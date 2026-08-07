using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A2GestampApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectFolderToInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectFolder",
                table: "inspection",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectFolder",
                table: "inspection");
        }
    }
}
