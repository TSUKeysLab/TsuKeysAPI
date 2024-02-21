using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tsuKeysAPIProject.Migrations
{
    /// <inheritdoc />
    public partial class keysFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndOfBooking",
                table: "KeyRequest");

            migrationBuilder.RenameColumn(
                name: "Owner",
                table: "Keys",
                newName: "OwnerEmail");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "KeyRequest",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "KeyRequest");

            migrationBuilder.RenameColumn(
                name: "OwnerEmail",
                table: "Keys",
                newName: "Owner");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndOfBooking",
                table: "KeyRequest",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
