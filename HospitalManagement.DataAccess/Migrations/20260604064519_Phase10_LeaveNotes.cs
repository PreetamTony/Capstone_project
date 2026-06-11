using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Phase10_LeaveNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "DoctorLeaves",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "DoctorLeaves");
        }
    }
}
