using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tsuKeysAPIProject.Migrations
{
    /// <inheritdoc />
    public partial class RequestOwnerRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ownerRole",
                table: "Requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ownerRole",
                table: "Requests");
        }
    }
}
