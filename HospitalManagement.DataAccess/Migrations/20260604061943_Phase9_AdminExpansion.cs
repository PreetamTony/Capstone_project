using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Phase9_AdminExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "InsuranceClaims",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "HeadDoctorId",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HeadDoctorId1",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadDoctorId1",
                table: "Departments",
                column: "HeadDoctorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Doctors_HeadDoctorId1",
                table: "Departments",
                column: "HeadDoctorId1",
                principalTable: "Doctors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Doctors_HeadDoctorId1",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HeadDoctorId1",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HeadDoctorId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HeadDoctorId1",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "InsuranceClaims",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
