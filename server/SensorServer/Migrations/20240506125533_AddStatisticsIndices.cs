using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorServer.Migrations
{
    /// <inheritdoc />
    public partial class AddStatisticsIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Measurements_HumidityPercent",
                table: "Measurements",
                column: "HumidityPercent");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_PressureHectopascals",
                table: "Measurements",
                column: "PressureHectopascals");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_TemperatureCelsius",
                table: "Measurements",
                column: "TemperatureCelsius");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Measurements_HumidityPercent",
                table: "Measurements");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_PressureHectopascals",
                table: "Measurements");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_TemperatureCelsius",
                table: "Measurements");
        }
    }
}
