CREATE TABLE dbo.Accounts
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Accounts PRIMARY KEY,
    UserId uniqueidentifier NOT NULL,
    Name nvarchar(100) NOT NULL,
    Type tinyint NOT NULL CONSTRAINT CK_Accounts_Type CHECK (Type BETWEEN 1 AND 4),
    InitialBalance decimal(19,2) NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Accounts_IsActive DEFAULT (1),
    CreatedAt datetimeoffset(0) NOT NULL,
    CONSTRAINT FK_Accounts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_Accounts_User_Name UNIQUE (UserId, Name)
);

CREATE TABLE dbo.Categories
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
    UserId uniqueidentifier NOT NULL,
    Name nvarchar(80) NOT NULL,
    Type tinyint NOT NULL CONSTRAINT CK_Categories_Type CHECK (Type IN (1, 2)),
    Color char(7) NOT NULL,
    CreatedAt datetimeoffset(0) NOT NULL,
    CONSTRAINT FK_Categories_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_Categories_User_Type_Name UNIQUE (UserId, Type, Name),
    CONSTRAINT CK_Categories_Color CHECK (Color LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
);
