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
