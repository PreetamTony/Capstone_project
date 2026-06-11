using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class QueueModuleRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CallCount",
                table: "QueueEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallCount",
                table: "QueueEntries");
        }
    }
}
