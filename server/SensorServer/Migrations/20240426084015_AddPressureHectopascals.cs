using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPressureHectopascals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PressureHectopascals",
                table: "Measurements",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PressureHectopascals",
                table: "Measurements");
        }
    }
}
