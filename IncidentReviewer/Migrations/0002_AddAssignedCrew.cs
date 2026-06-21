using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncidentReviewer.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedCrew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedCrew",
                table: "Incidents",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedCrew",
                table: "Incidents");
        }
    }
}
