using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Songbook.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSongModerationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HiddenReason",
                table: "Songs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHiddenByAdmin",
                table: "Songs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HiddenReason",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "IsHiddenByAdmin",
                table: "Songs");
        }
    }
}
