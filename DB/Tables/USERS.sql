CREATE TABLE [dbo].[USERS] (
    [Id] int NOT NULL CONSTRAINT [DF_USERS_Id] DEFAULT (NEXT VALUE FOR [dbo].[UserIdSequence]),
    [Name] nvarchar(120) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [PasswordHash] nvarchar(512) NOT NULL,
    [DepartmentId] int NOT NULL,
    [Role] nvarchar(60) NOT NULL,
    CONSTRAINT [PK_USERS] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_USERS_DEPARTMENTS_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[DEPARTMENTS] ([Id])
);

GO

CREATE UNIQUE INDEX [IX_USERS_Email]
    ON [dbo].[USERS] ([Email]);

GO

CREATE INDEX [IX_USERS_DepartmentId]
    ON [dbo].[USERS] ([DepartmentId]);

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'系統使用者資料表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'USERS';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'使用者識別碼 (主鍵)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'USERS', @level2type = N'COLUMN', @level2name = N'Id';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'使用者姓名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'USERS', @level2type = N'COLUMN', @level2name = N'Name';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'電子郵件地址 (登入帳號，唯一索引)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'USERS', @level2type = N'COLUMN', @level2name = N'Email';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'BCrypt 密碼雜湊值 (不儲存明碼)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'USERS', @level2type = N'COLUMN', @level2name = N'PasswordHash';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所屬部門識別碼 (外鍵關聯 DEPARTMENTS)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'USERS', @level2type = N'COLUMN', @level2name = N'DepartmentId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'使用者角色 (如 Owner, Member)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'USERS', @level2type = N'COLUMN', @level2name = N'Role';

GO
