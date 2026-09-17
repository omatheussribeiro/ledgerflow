IF COL_LENGTH('dbo.Accounts', 'DeletedAt') IS NULL
    ALTER TABLE dbo.Accounts ADD DeletedAt datetimeoffset(0) NULL;

IF COL_LENGTH('dbo.Categories', 'DeletedAt') IS NULL
    ALTER TABLE dbo.Categories ADD DeletedAt datetimeoffset(0) NULL;

IF COL_LENGTH('dbo.Transactions', 'DeletedAt') IS NULL
    ALTER TABLE dbo.Transactions ADD DeletedAt datetimeoffset(0) NULL;
GO

IF EXISTS (
    SELECT 1
    FROM sys.key_constraints
    WHERE [name] = 'UQ_Accounts_User_Name'
      AND parent_object_id = OBJECT_ID('dbo.Accounts')
)
    ALTER TABLE dbo.Accounts DROP CONSTRAINT UQ_Accounts_User_Name;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = 'UQ_Accounts_User_Name'
      AND object_id = OBJECT_ID('dbo.Accounts')
)
    CREATE UNIQUE INDEX UQ_Accounts_User_Name
        ON dbo.Accounts (UserId, Name)
        WHERE DeletedAt IS NULL;

IF EXISTS (
    SELECT 1
    FROM sys.key_constraints
    WHERE [name] = 'UQ_Categories_User_Type_Name'
      AND parent_object_id = OBJECT_ID('dbo.Categories')
)
    ALTER TABLE dbo.Categories DROP CONSTRAINT UQ_Categories_User_Type_Name;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = 'UQ_Categories_User_Type_Name'
      AND object_id = OBJECT_ID('dbo.Categories')
)
    CREATE UNIQUE INDEX UQ_Categories_User_Type_Name
        ON dbo.Categories (UserId, Type, Name)
        WHERE DeletedAt IS NULL;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = 'IX_Accounts_User_DeletedAt'
      AND object_id = OBJECT_ID('dbo.Accounts')
)
    CREATE INDEX IX_Accounts_User_DeletedAt
        ON dbo.Accounts (UserId, DeletedAt)
        INCLUDE (IsActive, Name, Type, InitialBalance);

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = 'IX_Categories_User_DeletedAt'
      AND object_id = OBJECT_ID('dbo.Categories')
)
    CREATE INDEX IX_Categories_User_DeletedAt
        ON dbo.Categories (UserId, DeletedAt)
        INCLUDE (Name, Type, Color);
