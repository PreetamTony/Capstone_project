using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Phase12_AppointmentOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "BookedByRole",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CheckInByUserId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationSentAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsultationRoom",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTeleConsultation",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotificationSentAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingProvider",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingUrl",
                table: "Appointments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueNumber",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReminderStatus",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RescheduledAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RescheduledByUserId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_AppointmentTime",
                table: "Appointments",
                columns: new[] { "PatientId", "AppointmentTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_QueueNumber",
                table: "Appointments",
                column: "QueueNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status",
                table: "Appointments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId_AppointmentTime",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_QueueNumber",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Status",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "BookedByRole",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CheckInByUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConfirmationSentAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConsultationRoom",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsTeleConsultation",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "LastNotificationSentAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "MeetingProvider",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "MeetingUrl",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "QueueNumber",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderStatus",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "RescheduledAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "RescheduledByUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");
        }
    }
}
