using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "USERS",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2b$10$ucv6PQN2oV/TVOsyYpXKhOrPNJd4Pq4MDGdm6VzK4tHfS6kVUkAza");

            migrationBuilder.UpdateData(
                table: "USERS",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2b$10$ucv6PQN2oV/TVOsyYpXKhOrPNJd4Pq4MDGdm6VzK4tHfS6kVUkAza");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "USERS",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "SEED_PASSWORD_HASH_REPLACE_BEFORE_USE");

            migrationBuilder.UpdateData(
                table: "USERS",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "SEED_PASSWORD_HASH_REPLACE_BEFORE_USE");
        }
    }
}
