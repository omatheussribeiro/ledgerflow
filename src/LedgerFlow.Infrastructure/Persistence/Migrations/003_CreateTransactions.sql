CREATE TABLE dbo.Transactions
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Transactions PRIMARY KEY,
    UserId uniqueidentifier NOT NULL,
    AccountId uniqueidentifier NOT NULL,
    CategoryId uniqueidentifier NOT NULL,
    Type tinyint NOT NULL CONSTRAINT CK_Transactions_Type CHECK (Type IN (1, 2)),
    Description nvarchar(160) NOT NULL,
    Amount decimal(19,2) NOT NULL CONSTRAINT CK_Transactions_Amount CHECK (Amount > 0),
    OccurredOn date NOT NULL,
    Status tinyint NOT NULL CONSTRAINT CK_Transactions_Status CHECK (Status BETWEEN 1 AND 3),
    Notes nvarchar(500) NULL,
    TransferGroupId uniqueidentifier NULL,
    RecurrenceId uniqueidentifier NULL,
    CreatedAt datetimeoffset(0) NOT NULL,
    CONSTRAINT FK_Transactions_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Transactions_Accounts FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Transactions_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
);
