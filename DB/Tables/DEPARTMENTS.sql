CREATE TABLE [dbo].[DEPARTMENTS] (
    [Id] int NOT NULL CONSTRAINT [DF_DEPARTMENTS_Id] DEFAULT (NEXT VALUE FOR [dbo].[DepartmentIdSequence]),
    [Name] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_DEPARTMENTS] PRIMARY KEY ([Id])
);

GO

CREATE UNIQUE INDEX [IX_DEPARTMENTS_Name]
    ON [dbo].[DEPARTMENTS] ([Name]);

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'部門資料表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DEPARTMENTS';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'部門識別碼 (主鍵)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DEPARTMENTS', @level2type = N'COLUMN', @level2name = N'Id';

GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'部門名稱 (唯一索引)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DEPARTMENTS', @level2type = N'COLUMN', @level2name = N'Name';

GO
