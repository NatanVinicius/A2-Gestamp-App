using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A2GestampApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inspection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OperatorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EmployeeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OperatorRole = table.Column<int>(type: "INTEGER", nullable: true),
                    FirstImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    SecondImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    ThirdImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalJudgement = table.Column<string>(type: "TEXT", nullable: false),
                    FinalJudgement = table.Column<string>(type: "TEXT", nullable: false),
                    ProductionShiftId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inspection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "production_shift",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShiftNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Produced = table.Column<int>(type: "INTEGER", nullable: false),
                    Approved = table.Column<int>(type: "INTEGER", nullable: false),
                    Reproved = table.Column<int>(type: "INTEGER", nullable: false),
                    LastInspectionResult = table.Column<int>(type: "INTEGER", nullable: false),
                    LastCycleTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_shift", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inspection");

            migrationBuilder.DropTable(
                name: "production_shift");
        }
    }
}
