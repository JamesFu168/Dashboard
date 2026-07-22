using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dashboard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DEPARTMENTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEPARTMENTS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_USERS_DEPARTMENTS_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DEPARTMENTS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CARDS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    DevOpsUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CARDS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CARDS_DEPARTMENTS_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DEPARTMENTS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CARDS_USERS_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REFRESH_TOKENS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REFRESH_TOKENS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_REFRESH_TOKENS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CARD_TASKS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    AssigneeId = table.Column<int>(type: "int", nullable: true),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DevOpsUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CARD_TASKS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CARD_TASKS_CARDS_CardId",
                        column: x => x.CardId,
                        principalTable: "CARDS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CARD_TASKS_USERS_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DEPARTMENTS",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Engineering" },
                    { 2, "Product" }
                });

            migrationBuilder.InsertData(
                table: "USERS",
                columns: new[] { "Id", "DepartmentId", "Email", "Name", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, 1, "alex@example.com", "Alex Owner", "SEED_PASSWORD_HASH_REPLACE_BEFORE_USE", "Owner" },
                    { 2, 1, "sam@example.com", "Sam Assignee", "SEED_PASSWORD_HASH_REPLACE_BEFORE_USE", "Member" }
                });

            migrationBuilder.InsertData(
                table: "CARDS",
                columns: new[] { "Id", "CreatedAt", "DepartmentId", "Description", "DevOpsUrl", "DueDate", "OwnerId", "Scope", "SequenceOrder", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 7, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null, "Seed card for personal board validation.", null, new DateOnly(2026, 8, 15), 1, 0, 100, 0, "Draft personal dashboard card", new DateTime(2026, 7, 22, 8, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2026, 7, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), 1, "Seed card for organization board validation.", null, new DateOnly(2026, 8, 20), 1, 1, 100, 1, "Coordinate organization workflow", new DateTime(2026, 7, 22, 8, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "CARD_TASKS",
                columns: new[] { "Id", "AssigneeId", "CardId", "CreatedAt", "DevOpsUrl", "DueDate", "IsCompleted", "SequenceOrder", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), 1, new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 7, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, 100, "Confirm API model", new DateTime(2026, 7, 22, 8, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000002"), 2, new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2026, 7, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, 100, "Review task permissions", new DateTime(2026, 7, 22, 8, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CARD_TASKS_AssigneeId",
                table: "CARD_TASKS",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_CARD_TASKS_CardId_SequenceOrder",
                table: "CARD_TASKS",
                columns: new[] { "CardId", "SequenceOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CARDS_DepartmentId",
                table: "CARDS",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CARDS_OwnerId",
                table: "CARDS",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CARDS_Status_SequenceOrder",
                table: "CARDS",
                columns: new[] { "Status", "SequenceOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DEPARTMENTS_Name",
                table: "DEPARTMENTS",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_Token",
                table: "REFRESH_TOKENS",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_UserId",
                table: "REFRESH_TOKENS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_DepartmentId",
                table: "USERS",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_Email",
                table: "USERS",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CARD_TASKS");

            migrationBuilder.DropTable(
                name: "REFRESH_TOKENS");

            migrationBuilder.DropTable(
                name: "CARDS");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "DEPARTMENTS");
        }
    }
}
