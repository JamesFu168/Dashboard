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
    [IsDeleted] bit NOT NULL CONSTRAINT [DF_CARDS_IsDeleted] DEFAULT ((0)),
    [DeletedAt] datetime NULL,
    [CreatedAt] datetime NOT NULL CONSTRAINT [DF_CARDS_CreatedAt] DEFAULT ([dbo].[GetTaiwanDate]()),
    [UpdatedAt] datetime NOT NULL,
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

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'看板卡片主表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片識別碼 (GUID 主鍵)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Id';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片標題', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Title';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片詳細描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Description';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片狀態欄位 (0:Plan, 1:ToDo, 2:Doing, 3:Done)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Status';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片可見範疇 (0:Personal, 1:Organization)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'Scope';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片擁有者識別碼 (外鍵關聯 USERS)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'OwnerId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所屬部門識別碼 (外鍵關聯 DEPARTMENTS，當 Scope=Organization 時有值)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'DepartmentId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片到期日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'DueDate';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'於欄位內部的顯示排序次序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'SequenceOrder';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'關聯之 Azure DevOps 工作項目網址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'DevOpsUrl';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已軟刪除 (0:否, 1:是，配合全域 Query Filter)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'IsDeleted';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'軟刪除時間 (台灣時間 UTC+8)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'DeletedAt';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'卡片建立時間 (台灣時間 UTC+8)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'CreatedAt';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'最後更新時間 (台灣時間 UTC+8，用於樂觀鎖比對)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARDS', @level2type = N'COLUMN', @level2name = N'UpdatedAt';

GO
