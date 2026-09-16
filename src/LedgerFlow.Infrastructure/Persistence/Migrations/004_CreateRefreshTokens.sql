CREATE TABLE dbo.RefreshTokens
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
    UserId uniqueidentifier NOT NULL,
    TokenHash char(64) NOT NULL,
    ExpiresAt datetimeoffset(0) NOT NULL,
    CreatedAt datetimeoffset(0) NOT NULL,
    RevokedAt datetimeoffset(0) NULL,
    ReplacedByTokenHash char(64) NULL,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_RefreshTokens_TokenHash UNIQUE (TokenHash)
);
