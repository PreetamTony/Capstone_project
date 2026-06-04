using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Appointments_AppointmentId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_LabReports_Appointments_AppointmentId",
                table: "LabReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Appointments_AppointmentId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Vitals_Appointments_AppointmentId",
                table: "Vitals");

            migrationBuilder.RenameColumn(
                name: "AppointmentId",
                table: "Vitals",
                newName: "VisitId");

            migrationBuilder.RenameIndex(
                name: "IX_Vitals_AppointmentId",
                table: "Vitals",
                newName: "IX_Vitals_VisitId");

            migrationBuilder.RenameColumn(
                name: "AppointmentId",
                table: "Prescriptions",
                newName: "VisitId");

            migrationBuilder.RenameIndex(
                name: "IX_Prescriptions_AppointmentId",
                table: "Prescriptions",
                newName: "IX_Prescriptions_VisitId");

            migrationBuilder.RenameColumn(
                name: "AppointmentId",
                table: "LabReports",
                newName: "VisitId");

            migrationBuilder.RenameIndex(
                name: "IX_LabReports_AppointmentId",
                table: "LabReports",
                newName: "IX_LabReports_VisitId");

            migrationBuilder.RenameColumn(
                name: "AppointmentId",
                table: "Bills",
                newName: "VisitId");

            migrationBuilder.RenameIndex(
                name: "IX_Bills_AppointmentId",
                table: "Bills",
                newName: "IX_Bills_VisitId");

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DischargeTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChiefComplaint = table.Column<string>(type: "text", nullable: true),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    ClinicalNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Visits_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AppointmentId",
                table: "Visits",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DoctorId",
                table: "Visits",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_PatientId",
                table: "Visits",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Visits_VisitId",
                table: "Bills",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabReports_Visits_VisitId",
                table: "LabReports",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Visits_VisitId",
                table: "Prescriptions",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vitals_Visits_VisitId",
                table: "Vitals",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Visits_VisitId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_LabReports_Visits_VisitId",
                table: "LabReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Visits_VisitId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Vitals_Visits_VisitId",
                table: "Vitals");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.RenameColumn(
                name: "VisitId",
                table: "Vitals",
                newName: "AppointmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Vitals_VisitId",
                table: "Vitals",
                newName: "IX_Vitals_AppointmentId");

            migrationBuilder.RenameColumn(
                name: "VisitId",
                table: "Prescriptions",
                newName: "AppointmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Prescriptions_VisitId",
                table: "Prescriptions",
                newName: "IX_Prescriptions_AppointmentId");

            migrationBuilder.RenameColumn(
                name: "VisitId",
                table: "LabReports",
                newName: "AppointmentId");

            migrationBuilder.RenameIndex(
                name: "IX_LabReports_VisitId",
                table: "LabReports",
                newName: "IX_LabReports_AppointmentId");

            migrationBuilder.RenameColumn(
                name: "VisitId",
                table: "Bills",
                newName: "AppointmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Bills_VisitId",
                table: "Bills",
                newName: "IX_Bills_AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Appointments_AppointmentId",
                table: "Bills",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabReports_Appointments_AppointmentId",
                table: "LabReports",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Appointments_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vitals_Appointments_AppointmentId",
                table: "Vitals",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
