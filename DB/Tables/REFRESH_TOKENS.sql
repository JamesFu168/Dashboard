CREATE TABLE [dbo].[REFRESH_TOKENS] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] int NOT NULL,
    [Token] nvarchar(512) NOT NULL,
    [ExpiresAt] datetime NOT NULL,
    [CreatedAt] datetime NOT NULL CONSTRAINT [DF_REFRESH_TOKENS_CreatedAt] DEFAULT ([dbo].[GetTaiwanDate]()),
    [IsRevoked] bit NOT NULL,
    CONSTRAINT [PK_REFRESH_TOKENS] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_REFRESH_TOKENS_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[USERS] ([Id]) ON DELETE CASCADE
);

GO

CREATE UNIQUE INDEX [IX_REFRESH_TOKENS_Token]
    ON [dbo].[REFRESH_TOKENS] ([Token]);

GO

CREATE INDEX [IX_REFRESH_TOKENS_UserId]
    ON [dbo].[REFRESH_TOKENS] ([UserId]);

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Refresh Token 識別碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'Id';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'使用者識別碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'UserId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Refresh Token 權杖字串', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'Token';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'過期時間', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'ExpiresAt';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'建立時間', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'CreatedAt';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已撤銷', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'IsRevoked';
