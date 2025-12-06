using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineClub.Migrations.CineDb
{
    /// <inheritdoc />
    public partial class AddUpdatedAtUtcToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Reviews",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Reviews");
        }
    }
}
