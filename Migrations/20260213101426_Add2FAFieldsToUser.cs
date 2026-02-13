using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestion_d_universitaire.Migrations
{
    /// <inheritdoc />
    public partial class Add2FAFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code2FA",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "Expiration2FA",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code2FA",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Expiration2FA",
                table: "AspNetUsers");
        }
    }
}
