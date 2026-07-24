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

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'JWT 重新整理權杖資料表 (Refresh Token)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Refresh Token 紀錄識別碼 (GUID 主鍵)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'Id';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所屬使用者識別碼 (外鍵關聯 USERS)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'UserId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Refresh Token 加密權杖字串 (唯一索引)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'Token';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'權杖過期時間 (台灣時間 UTC+8)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'ExpiresAt';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'權杖建立時間 (台灣時間 UTC+8)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'CreatedAt';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'權杖是否已被撤銷 (0:有效, 1:已撤銷)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'REFRESH_TOKENS', @level2type = N'COLUMN', @level2name = N'IsRevoked';

GO
