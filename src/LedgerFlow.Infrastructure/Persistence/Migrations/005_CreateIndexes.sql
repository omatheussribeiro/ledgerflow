-- Covers the dashboard, chronological feed, period filters and pagination per tenant.
CREATE INDEX IX_Transactions_User_OccurredOn
    ON dbo.Transactions (UserId, OccurredOn DESC)
    INCLUDE (AccountId, CategoryId, Type, Status, Amount, Description, CreatedAt);

-- Supports account balance aggregation without scanning another user's rows.
CREATE INDEX IX_Transactions_Account_Status
    ON dbo.Transactions (AccountId, Status)
    INCLUDE (Type, Amount);

-- Supports expense-by-category reports for a period.
CREATE INDEX IX_Transactions_User_Type_OccurredOn
    ON dbo.Transactions (UserId, Type, OccurredOn)
    INCLUDE (CategoryId, Status, Amount);

-- Keeps token cleanup and expiry checks efficient; token lookup itself uses its unique index.
CREATE INDEX IX_RefreshTokens_User_ExpiresAt
    ON dbo.RefreshTokens (UserId, ExpiresAt)
    INCLUDE (RevokedAt);
