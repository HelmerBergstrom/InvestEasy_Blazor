using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestEasy.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmExistingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Email confirmation is required from now on. Accounts created before that couldn't be
            // confirmed, since the app had no email sender, so they are marked as confirmed.
            migrationBuilder.Sql("UPDATE \"AspNetUsers\" SET \"EmailConfirmed\" = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
