using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EmrRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelationship",
                table: "Patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OrganDonorFlag",
                table: "Patients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Allergies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EmrDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmrRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlobUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmrDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmrDocuments_EmrRecords_EmrRecordId",
                        column: x => x.EmrRecordId,
                        principalTable: "EmrRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Immunizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmrRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    VaccineName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateAdministered = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DoseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Immunizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Immunizations_EmrRecords_EmrRecordId",
                        column: x => x.EmrRecordId,
                        principalTable: "EmrRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmrDocuments_EmrRecordId",
                table: "EmrDocuments",
                column: "EmrRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Immunizations_EmrRecordId",
                table: "Immunizations",
                column: "EmrRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmrDocuments");

            migrationBuilder.DropTable(
                name: "Immunizations");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationship",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "OrganDonorFlag",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Allergies");
        }
    }
}
