using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tsuKeysAPIProject.Migrations
{
    /// <inheritdoc />
    public partial class keyRequestFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_KeyRequest",
                table: "KeyRequest");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "KeyRequest",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_KeyRequest",
                table: "KeyRequest",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_KeyRequest",
                table: "KeyRequest");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "KeyRequest");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KeyRequest",
                table: "KeyRequest",
                column: "ClassroomNumber");
        }
    }
}
