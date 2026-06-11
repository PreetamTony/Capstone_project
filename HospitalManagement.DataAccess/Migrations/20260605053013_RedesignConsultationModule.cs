using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RedesignConsultationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabReports_Visits_VisitId",
                table: "LabReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Visits_VisitId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Consultations_ConsultationId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_ConsultationId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ConsultationId",
                table: "Visits");

            migrationBuilder.RenameColumn(
                name: "VisitId",
                table: "Prescriptions",
                newName: "ConsultationId");

            migrationBuilder.RenameIndex(
                name: "IX_Prescriptions_VisitId",
                table: "Prescriptions",
                newName: "IX_Prescriptions_ConsultationId");

            migrationBuilder.RenameColumn(
                name: "VisitId",
                table: "LabReports",
                newName: "ConsultationId");

            migrationBuilder.RenameIndex(
                name: "IX_LabReports_VisitId",
                table: "LabReports",
                newName: "IX_LabReports_ConsultationId");

            migrationBuilder.DropColumn(
                name: "Symptoms",
                table: "Consultations");

            migrationBuilder.AddColumn<List<string>>(
                name: "Symptoms",
                table: "Consultations",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Consultations",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Assessment",
                table: "Consultations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChiefComplaint",
                table: "Consultations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Consultations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisCode",
                table: "Consultations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                table: "Consultations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUpDate",
                table: "Consultations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowUpInstructions",
                table: "Consultations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Consultations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TreatmentPlan",
                table: "Consultations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VisitId",
                table: "Consultations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_CreatedAt",
                table: "Consultations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorId",
                table: "Consultations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_FollowUpDate",
                table: "Consultations",
                column: "FollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_Status",
                table: "Consultations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_VisitId",
                table: "Consultations",
                column: "VisitId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Doctors_DoctorId",
                table: "Consultations",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Visits_VisitId",
                table: "Consultations",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabReports_Consultations_ConsultationId",
                table: "LabReports",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Consultations_ConsultationId",
                table: "Prescriptions",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Doctors_DoctorId",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Visits_VisitId",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_LabReports_Consultations_ConsultationId",
                table: "LabReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Consultations_ConsultationId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_CreatedAt",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_DoctorId",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_FollowUpDate",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_Status",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_VisitId",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "Assessment",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "ChiefComplaint",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "DiagnosisCode",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "FollowUpInstructions",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "TreatmentPlan",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "Consultations");

            migrationBuilder.RenameColumn(
                name: "ConsultationId",
                table: "Prescriptions",
                newName: "VisitId");

            migrationBuilder.RenameIndex(
                name: "IX_Prescriptions_ConsultationId",
                table: "Prescriptions",
                newName: "IX_Prescriptions_VisitId");

            migrationBuilder.RenameColumn(
                name: "ConsultationId",
                table: "LabReports",
                newName: "VisitId");

            migrationBuilder.RenameIndex(
                name: "IX_LabReports_ConsultationId",
                table: "LabReports",
                newName: "IX_LabReports_VisitId");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsultationId",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Symptoms",
                table: "Consultations",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Consultations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ConsultationId",
                table: "Visits",
                column: "ConsultationId",
                unique: true);

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
                name: "FK_Visits_Consultations_ConsultationId",
                table: "Visits",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
