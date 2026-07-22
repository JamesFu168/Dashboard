INSERT INTO [DEPARTMENTS] ([Id], [Name])
VALUES (1, N'Engineering'), (2, N'Product');
ALTER SEQUENCE [dbo].[DepartmentIdSequence] RESTART WITH 3;

INSERT INTO [USERS] ([Id], [Name], [Email], [DepartmentId], [Role])
VALUES
    (1, N'Alex Owner', N'alex@example.com', 1, N'Owner'),
    (2, N'Sam Assignee', N'sam@example.com', 1, N'Member');
ALTER SEQUENCE [dbo].[UserIdSequence] RESTART WITH 3;

INSERT INTO [CARDS] (
    [Id], [Title], [Description], [Status], [Scope], [OwnerId], [DepartmentId],
    [DueDate], [SequenceOrder], [DevOpsUrl], [CreatedAt], [UpdatedAt]
)
VALUES
    ('10000000-0000-0000-0000-000000000001', N'Draft personal dashboard card', N'Seed card for personal board validation.', 0, 0, 1, NULL, '2026-08-15', 100, NULL, '2026-07-22T08:00:00Z', '2026-07-22T08:00:00Z'),
    ('10000000-0000-0000-0000-000000000002', N'Coordinate organization workflow', N'Seed card for organization board validation.', 1, 1, 1, 1, '2026-08-20', 100, NULL, '2026-07-22T08:00:00Z', '2026-07-22T08:00:00Z');

INSERT INTO [CARD_TASKS] (
    [Id], [CardId], [Title], [IsCompleted], [AssigneeId], [SequenceOrder],
    [DueDate], [DevOpsUrl], [CreatedAt], [UpdatedAt]
)
VALUES
    ('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', N'Confirm API model', 0, 1, 100, NULL, NULL, '2026-07-22T08:00:00Z', '2026-07-22T08:00:00Z'),
    ('20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000002', N'Review task permissions', 0, 2, 100, NULL, NULL, '2026-07-22T08:00:00Z', '2026-07-22T08:00:00Z');
