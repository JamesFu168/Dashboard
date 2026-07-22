CREATE TABLE [dbo].[CARDS] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(4000) NULL,
    [Status] int NOT NULL,
    [Scope] int NOT NULL,
    [OwnerId] int NOT NULL,
    [DepartmentId] int NULL,
    [DueDate] date NULL,
    [SequenceOrder] int NOT NULL,
    [DevOpsUrl] nvarchar(1000) NULL,
    [CreatedAt] datetimeoffset(3) NOT NULL,
    [UpdatedAt] datetimeoffset(3) NOT NULL,
    CONSTRAINT [PK_CARDS] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CARDS_DEPARTMENTS_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[DEPARTMENTS] ([Id]),
    CONSTRAINT [FK_CARDS_USERS_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[USERS] ([Id])
);

GO

CREATE INDEX [IX_CARDS_DepartmentId]
    ON [dbo].[CARDS] ([DepartmentId]);

GO

CREATE INDEX [IX_CARDS_OwnerId]
    ON [dbo].[CARDS] ([OwnerId]);

GO

CREATE INDEX [IX_CARDS_Status_SequenceOrder]
    ON [dbo].[CARDS] ([Status], [SequenceOrder]);

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片識別碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Id';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片標題', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Title';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Description';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片狀態', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Status';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片範圍', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Scope';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'負責人識別碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'OwnerId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所屬部門識別碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'DepartmentId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'DueDate';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'顯示排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'SequenceOrder';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Azure DevOps 連結', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'DevOpsUrl';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'建立時間', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'CreatedAt';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'最後更新時間', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
