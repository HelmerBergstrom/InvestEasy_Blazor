using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestEasy.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchListItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "WatchListItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WatchListItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "WatchListItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "WatchListItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "WatchListItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "WatchListItems");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "WatchListItems");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "WatchListItems");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "WatchListItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "WatchListItems");
        }
    }
}
