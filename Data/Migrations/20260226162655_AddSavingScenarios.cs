using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestEasy.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingScenarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskProfiles_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SavingScenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    InitialAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    SavingHorizon = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpectedReturnPercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingScenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingScenarios_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProgresses_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WatchListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchListItems_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskProfiles_ApplicationUserId",
                table: "RiskProfiles",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingScenarios_ApplicationUserId",
                table: "SavingScenarios",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_ApplicationUserId",
                table: "UserProgresses",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListItems_ApplicationUserId",
                table: "WatchListItems",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskProfiles");

            migrationBuilder.DropTable(
                name: "SavingScenarios");

            migrationBuilder.DropTable(
                name: "UserProgresses");

            migrationBuilder.DropTable(
                name: "WatchListItems");
        }
    }
}
