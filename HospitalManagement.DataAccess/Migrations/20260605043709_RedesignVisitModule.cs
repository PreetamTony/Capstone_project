using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RedesignVisitModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Visits_VisitId",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_VisitId",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "Consultations");

            migrationBuilder.RenameColumn(
                name: "Diagnosis",
                table: "Visits",
                newName: "RoomNumber");

            migrationBuilder.RenameColumn(
                name: "ClinicalNotes",
                table: "Visits",
                newName: "QueueNumber");

            migrationBuilder.AddColumn<Guid>(
                name: "BillingId",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BillingId1",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Visits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledBy",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConsultationId",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitNumber",
                table: "Visits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisitType",
                table: "Visits",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VisitHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousState = table.Column<string>(type: "text", nullable: false),
                    NewState = table.Column<string>(type: "text", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitHistories_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_BillingId1",
                table: "Visits",
                column: "BillingId1");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CheckInTime",
                table: "Visits",
                column: "CheckInTime");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ConsultationId",
                table: "Visits",
                column: "ConsultationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DepartmentId",
                table: "Visits",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_Status",
                table: "Visits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_VisitNumber",
                table: "Visits",
                column: "VisitNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_VisitType",
                table: "Visits",
                column: "VisitType");

            migrationBuilder.CreateIndex(
                name: "IX_VisitHistories_VisitId",
                table: "VisitHistories",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Billings_BillingId1",
                table: "Visits",
                column: "BillingId1",
                principalTable: "Billings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Consultations_ConsultationId",
                table: "Visits",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Departments_DepartmentId",
                table: "Visits",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Billings_BillingId1",
                table: "Visits");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Consultations_ConsultationId",
                table: "Visits");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Departments_DepartmentId",
                table: "Visits");

            migrationBuilder.DropTable(
                name: "VisitHistories");

            migrationBuilder.DropIndex(
                name: "IX_Visits_BillingId1",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_CheckInTime",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_ConsultationId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_DepartmentId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_Status",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_VisitNumber",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_VisitType",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "BillingId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "BillingId1",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ConsultationId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VisitNumber",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VisitType",
                table: "Visits");

            migrationBuilder.RenameColumn(
                name: "RoomNumber",
                table: "Visits",
                newName: "Diagnosis");

            migrationBuilder.RenameColumn(
                name: "QueueNumber",
                table: "Visits",
                newName: "ClinicalNotes");

            migrationBuilder.AddColumn<Guid>(
                name: "VisitId",
                table: "Consultations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_VisitId",
                table: "Consultations",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Visits_VisitId",
                table: "Consultations",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
