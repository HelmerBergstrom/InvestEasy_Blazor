using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestEasy.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "RiskProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ExperienceLevel",
                table: "RiskProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FundsPercentage",
                table: "RiskProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvestmentGoal",
                table: "RiskProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarketDropReaction",
                table: "RiskProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimalRiskPercentage",
                table: "RiskProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileName",
                table: "RiskProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StocksPercentage",
                table: "RiskProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeHorizon",
                table: "RiskProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalScore",
                table: "RiskProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RiskProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "FundsPercentage",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "InvestmentGoal",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "MarketDropReaction",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "MinimalRiskPercentage",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "ProfileName",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "StocksPercentage",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "TimeHorizon",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "RiskProfiles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RiskProfiles");
        }
    }
}
