using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using LedgerFlow.Api.Configuration;
using LedgerFlow.Api.Errors;
using LedgerFlow.Api.Middleware;
using LedgerFlow.Api.Security;
using LedgerFlow.Application;
using LedgerFlow.Application.Dtos.Common;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Infrastructure;
using LedgerFlow.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(() => builder.Configuration.GetConnectionString("LedgerFlow")
    ?? throw new InvalidOperationException("ConnectionStrings:LedgerFlow is required."));
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Request validation failed",
            Detail = "One or more fields contain invalid or missing values.",
            Instance = context.HttpContext.Request.Path
        };
        ApiProblemDetailsWriter.Enrich(
            details,
            context.HttpContext,
            "validation_failed",
            details.Detail);
        return new BadRequestObjectResult(details)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    ApiProblemDetailsWriter.Enrich(context.ProblemDetails, context.HttpContext);
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
        ?? throw new InvalidOperationException("JWT configuration is required.");
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
        ClockSkew = TimeSpan.FromSeconds(30),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});
builder.Services.AddAuthorizationBuilder().AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(options => options.AddPolicy("web", policy => policy
    .WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LedgerFlow API",
        Version = "v1",
        Description = "API for authentication, accounts, categories, transactions and financial dashboards. Errors follow RFC 7807.",
        Contact = new OpenApiContact
        {
            Name = "LedgerFlow",
            Url = new Uri("https://github.com/omatheussribeiro/ledgerflow")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://github.com/omatheussribeiro/ledgerflow/blob/main/LICENSE")
        }
    });
    var documentedAssemblies = new[]
    {
        Assembly.GetExecutingAssembly(),
        typeof(IAuthService).Assembly
    };
    foreach (var assembly in documentedAssemblies)
    {
        var xmlDocumentation = $"{assembly.GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlDocumentation), true);
    }
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste only the JWT access token."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("LedgerFlow");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:LedgerFlow is required.");
var jwt = app.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is required.");
if (Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32 ||
    (!app.Environment.IsDevelopment() && jwt.SigningKey.StartsWith("CONFIGURE_", StringComparison.Ordinal)))
    throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes.");

if (app.Configuration.GetValue("Database:RunMigrations", true))
{
    app.Services.GetRequiredService<DatabaseInitializer>().Migrate();
    if (app.Environment.IsDevelopment() && app.Configuration.GetValue("Database:SeedDevelopmentData", true))
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
    }
}

app.UseMiddleware<RequestContextMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages(context => ApiProblemDetailsWriter.WriteStatusCodeAsync(context.HttpContext));
app.UseCors("web");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapGet("/health", () => Results.Ok(ApiResponseFactory.Success(
        new HealthStatusDto("healthy", DateTimeOffset.UtcNow),
        "The API is healthy.")))
    .AllowAnonymous()
    .WithName("HealthCheck")
    .WithSummary("Checks API availability")
    .WithDescription("Returns the API health status and the current UTC timestamp.")
    .Produces<ApiResponseDto<HealthStatusDto>>(StatusCodes.Status200OK);
app.Run();

public partial class Program;
