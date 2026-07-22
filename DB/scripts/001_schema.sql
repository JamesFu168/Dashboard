CREATE TABLE [DEPARTMENTS] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_DEPARTMENTS] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_DEPARTMENTS_Name] ON [DEPARTMENTS] ([Name]);

CREATE TABLE [USERS] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(120) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [DepartmentId] int NOT NULL,
    [Role] nvarchar(60) NOT NULL,
    CONSTRAINT [PK_USERS] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_USERS_DEPARTMENTS_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [DEPARTMENTS] ([Id])
);

CREATE UNIQUE INDEX [IX_USERS_Email] ON [USERS] ([Email]);
CREATE INDEX [IX_USERS_DepartmentId] ON [USERS] ([DepartmentId]);

CREATE TABLE [CARDS] (
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
    CONSTRAINT [FK_CARDS_DEPARTMENTS_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [DEPARTMENTS] ([Id]),
    CONSTRAINT [FK_CARDS_USERS_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [USERS] ([Id])
);

CREATE INDEX [IX_CARDS_DepartmentId] ON [CARDS] ([DepartmentId]);
CREATE INDEX [IX_CARDS_OwnerId] ON [CARDS] ([OwnerId]);
CREATE INDEX [IX_CARDS_Status_SequenceOrder] ON [CARDS] ([Status], [SequenceOrder]);

CREATE TABLE [CARD_TASKS] (
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
    CONSTRAINT [FK_CARD_TASKS_CARDS_CardId] FOREIGN KEY ([CardId]) REFERENCES [CARDS] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CARD_TASKS_USERS_AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [USERS] ([Id])
);

CREATE INDEX [IX_CARD_TASKS_AssigneeId] ON [CARD_TASKS] ([AssigneeId]);
CREATE INDEX [IX_CARD_TASKS_CardId_SequenceOrder] ON [CARD_TASKS] ([CardId], [SequenceOrder]);
