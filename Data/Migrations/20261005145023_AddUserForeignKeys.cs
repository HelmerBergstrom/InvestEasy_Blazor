using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestEasy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Removes rows that would break the new foreign keys and unique indexes:
            // data left behind by deleted accounts, and duplicates (the newest row is kept).
            foreach (var table in new[] { "SavingScenarios", "WatchListItems", "UserProgresses", "RiskProfiles" })
            {
                migrationBuilder.Sql($"DELETE FROM \"{table}\" WHERE \"UserId\" NOT IN (SELECT \"Id\" FROM \"AspNetUsers\");");
            }

            migrationBuilder.Sql("DELETE FROM \"WatchListItems\" WHERE \"Id\" NOT IN (SELECT MAX(\"Id\") FROM \"WatchListItems\" GROUP BY \"UserId\", \"Symbol\");");
            migrationBuilder.Sql("DELETE FROM \"UserProgresses\" WHERE \"Id\" NOT IN (SELECT MAX(\"Id\") FROM \"UserProgresses\" GROUP BY \"UserId\");");
            migrationBuilder.Sql("DELETE FROM \"RiskProfiles\" WHERE \"Id\" NOT IN (SELECT MAX(\"Id\") FROM \"RiskProfiles\" GROUP BY \"UserId\");");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskProfiles_AspNetUsers_ApplicationUserId",
                table: "RiskProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_SavingScenarios_AspNetUsers_ApplicationUserId",
                table: "SavingScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgresses_AspNetUsers_ApplicationUserId",
                table: "UserProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchListItems_AspNetUsers_ApplicationUserId",
                table: "WatchListItems");

            migrationBuilder.DropIndex(
                name: "IX_WatchListItems_ApplicationUserId",
                table: "WatchListItems");

            migrationBuilder.DropIndex(
                name: "IX_UserProgresses_ApplicationUserId",
                table: "UserProgresses");

            migrationBuilder.DropIndex(
                name: "IX_SavingScenarios_ApplicationUserId",
                table: "SavingScenarios");

            migrationBuilder.DropIndex(
                name: "IX_RiskProfiles_ApplicationUserId",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "WatchListItems");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "UserProgresses");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "SavingScenarios");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "RiskProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_UserId_Symbol",
                table: "WatchListItems",
                columns: new[] { "UserId", "Symbol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_UserId",
                table: "UserProgresses",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingScenarios_UserId",
                table: "SavingScenarios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskProfiles_UserId",
                table: "RiskProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskProfiles_AspNetUsers_UserId",
                table: "RiskProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavingScenarios_AspNetUsers_UserId",
                table: "SavingScenarios",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgresses_AspNetUsers_UserId",
                table: "UserProgresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchListItems_AspNetUsers_UserId",
                table: "WatchListItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskProfiles_AspNetUsers_UserId",
                table: "RiskProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_SavingScenarios_AspNetUsers_UserId",
                table: "SavingScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgresses_AspNetUsers_UserId",
                table: "UserProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchListItems_AspNetUsers_UserId",
                table: "WatchListItems");

            migrationBuilder.DropIndex(
                name: "IX_WatchListItems_UserId_Symbol",
                table: "WatchListItems");

            migrationBuilder.DropIndex(
                name: "IX_UserProgresses_UserId",
                table: "UserProgresses");

            migrationBuilder.DropIndex(
                name: "IX_SavingScenarios_UserId",
                table: "SavingScenarios");

            migrationBuilder.DropIndex(
                name: "IX_RiskProfiles_UserId",
                table: "RiskProfiles");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "WatchListItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "UserProgresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "SavingScenarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "RiskProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_ApplicationUserId",
                table: "WatchListItems",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_ApplicationUserId",
                table: "UserProgresses",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingScenarios_ApplicationUserId",
                table: "SavingScenarios",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskProfiles_ApplicationUserId",
                table: "RiskProfiles",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskProfiles_AspNetUsers_ApplicationUserId",
                table: "RiskProfiles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingScenarios_AspNetUsers_ApplicationUserId",
                table: "SavingScenarios",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgresses_AspNetUsers_ApplicationUserId",
                table: "UserProgresses",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WatchListItems_AspNetUsers_ApplicationUserId",
                table: "WatchListItems",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
