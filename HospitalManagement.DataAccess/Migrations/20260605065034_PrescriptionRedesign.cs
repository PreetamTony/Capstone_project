using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PrescriptionRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DispensedBy",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Dosage",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "EditableUntil",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "IsDispensed",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "MedicationName",
                table: "Prescriptions");

            migrationBuilder.RenameColumn(
                name: "VoidReason",
                table: "Prescriptions",
                newName: "Notes");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Prescriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAt",
                table: "Prescriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Prescriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Strength = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dosage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsDispensed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                table: "PrescriptionItems",
                column: "PrescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Prescriptions");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Prescriptions",
                newName: "VoidReason");

            migrationBuilder.AddColumn<Guid>(
                name: "DispensedBy",
                table: "Prescriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dosage",
                table: "Prescriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "Prescriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditableUntil",
                table: "Prescriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "Prescriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Prescriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDispensed",
                table: "Prescriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "Prescriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MedicationName",
                table: "Prescriptions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
