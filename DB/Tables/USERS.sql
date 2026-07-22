CREATE TABLE [dbo].[USERS] (
    [Id] int NOT NULL CONSTRAINT [DF_USERS_Id] DEFAULT (NEXT VALUE FOR [dbo].[UserIdSequence]),
    [Name] nvarchar(120) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
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
