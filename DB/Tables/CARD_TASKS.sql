CREATE TABLE [dbo].[CARD_TASKS] (
    [Id] uniqueidentifier NOT NULL,
    [CardId] uniqueidentifier NOT NULL,
    [Title] nvarchar(240) NOT NULL,
    [IsCompleted] bit NOT NULL,
    [AssigneeId] int NULL,
    [SequenceOrder] int NOT NULL,
    [DueDate] date NULL,
    [DevOpsUrl] nvarchar(1000) NULL,
    [CreatedAt] datetime NOT NULL CONSTRAINT [DF_CARD_TASKS_CreatedAt] DEFAULT ([dbo].[GetTaiwanDate]()),
    [UpdatedAt] datetime NOT NULL,
    CONSTRAINT [PK_CARD_TASKS] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CARD_TASKS_CARDS_CardId] FOREIGN KEY ([CardId]) REFERENCES [dbo].[CARDS] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CARD_TASKS_USERS_AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [dbo].[USERS] ([Id])
);

GO

CREATE INDEX [IX_CARD_TASKS_AssigneeId]
    ON [dbo].[CARD_TASKS] ([AssigneeId]);

GO

CREATE INDEX [IX_CARD_TASKS_CardId_SequenceOrder]
    ON [dbo].[CARD_TASKS] ([CardId], [SequenceOrder]);

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任務識別碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'Id';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所屬卡片識別碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'CardId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任務標題', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'Title';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已完成', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'IsCompleted';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'指派人識別碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'AssigneeId';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'顯示排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'SequenceOrder';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'DueDate';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Azure DevOps 連結', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'DevOpsUrl';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'建立時間', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'CreatedAt';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'最後更新時間', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CARD_TASKS', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
