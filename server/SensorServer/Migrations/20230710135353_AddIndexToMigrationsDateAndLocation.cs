using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorServer.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToMigrationsDateAndLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Date",
                table: "Measurements",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Location",
                table: "Measurements",
                column: "Location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Measurements_Date",
                table: "Measurements");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_Location",
                table: "Measurements");
        }
    }
}
