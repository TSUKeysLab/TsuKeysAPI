using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tsuKeysAPIProject.Migrations
{
    /// <inheritdoc />
    public partial class keyFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerEmail",
                table: "Keys",
                newName: "Owner");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Owner",
                table: "Keys",
                newName: "OwnerEmail");
        }
    }
}
