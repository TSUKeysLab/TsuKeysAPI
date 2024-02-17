using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tsuKeysAPIProject.Migrations
{
    /// <inheritdoc />
    public partial class RequestKeyTableAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyRequest",
                columns: table => new
                {
                    ClassroomNumber = table.Column<string>(type: "text", nullable: false),
                    KeyOwner = table.Column<string>(type: "text", nullable: false),
                    KeyRecipient = table.Column<string>(type: "text", nullable: false),
                    EndOfBooking = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyRequest", x => x.ClassroomNumber);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyRequest");
        }
    }
}
