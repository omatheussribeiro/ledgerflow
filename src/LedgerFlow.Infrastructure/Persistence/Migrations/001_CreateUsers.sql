CREATE TABLE dbo.Users
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Name nvarchar(120) NOT NULL,
    Email nvarchar(254) NOT NULL,
    NormalizedEmail nvarchar(254) NOT NULL,
    PasswordHash nvarchar(500) NOT NULL,
    Role varchar(20) NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN ('User', 'Admin')),
    CreatedAt datetimeoffset(0) NOT NULL,
    CONSTRAINT UQ_Users_NormalizedEmail UNIQUE (NormalizedEmail)
);
