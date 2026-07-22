CREATE TABLE [dbo].[CARD_TASKS] (
    [Id] uniqueidentifier NOT NULL,
    [CardId] uniqueidentifier NOT NULL,
    [Title] nvarchar(240) NOT NULL,
    [IsCompleted] bit NOT NULL,
    [AssigneeId] int NULL,
    [SequenceOrder] int NOT NULL,
    [DueDate] date NULL,
    [DevOpsUrl] nvarchar(1000) NULL,
    [CreatedAt] datetimeoffset(3) NOT NULL,
    [UpdatedAt] datetimeoffset(3) NOT NULL,
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
