using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLabOrdersAndReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceClaims_Patients_PatientId",
                table: "InsuranceClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceClaims_Visits_VisitId",
                table: "InsuranceClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Billings_BillingId1",
                table: "Visits");

            migrationBuilder.DropTable(
                name: "Billings");

            migrationBuilder.DropIndex(
                name: "IX_Visits_BillingId1",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceClaims_PatientId",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "BillingId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "BillingId1",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ClaimAmount",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "InsuranceClaims");

            migrationBuilder.RenameColumn(
                name: "VisitId",
                table: "InsuranceClaims",
                newName: "InvoiceId");

            migrationBuilder.RenameColumn(
                name: "Provider",
                table: "InsuranceClaims",
                newName: "InsuranceProvider");

            migrationBuilder.RenameColumn(
                name: "PolicyNumber",
                table: "InsuranceClaims",
                newName: "ClaimNumber");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceClaims_VisitId",
                table: "InsuranceClaims",
                newName: "IX_InsuranceClaims_InvoiceId");

            migrationBuilder.AddColumn<int>(
                name: "Channel",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceId",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OrderNotes",
                table: "LabReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "LabReports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "LabReports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "LabReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestName",
                table: "LabReports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedAmount",
                table: "InsuranceClaims",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "InsuranceClaims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "InsuranceClaims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RequestedAmount",
                table: "InsuranceClaims",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "InsuranceClaims",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "BillingAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    InsuranceCoverage = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PatientResponsibility = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    PatientId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Patients_PatientId1",
                        column: x => x.PatientId1,
                        principalTable: "Patients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    TransactionId = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refunds_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingAudits_EntityId",
                table: "BillingAudits",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DoctorId",
                table: "Invoices",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PatientId",
                table: "Invoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PatientId1",
                table: "Invoices",
                column: "PatientId1");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_VisitId",
                table: "Invoices",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_InvoiceId",
                table: "Refunds",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_PaymentId",
                table: "Refunds",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceClaims_Invoices_InvoiceId",
                table: "InsuranceClaims",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceClaims_Invoices_InvoiceId",
                table: "InsuranceClaims");

            migrationBuilder.DropTable(
                name: "BillingAudits");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "OrderNotes",
                table: "LabReports");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "LabReports");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "LabReports");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "LabReports");

            migrationBuilder.DropColumn(
                name: "TestName",
                table: "LabReports");

            migrationBuilder.DropColumn(
                name: "ApprovedAmount",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "RequestedAmount",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "InsuranceClaims");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "InsuranceClaims",
                newName: "VisitId");

            migrationBuilder.RenameColumn(
                name: "InsuranceProvider",
                table: "InsuranceClaims",
                newName: "Provider");

            migrationBuilder.RenameColumn(
                name: "ClaimNumber",
                table: "InsuranceClaims",
                newName: "PolicyNumber");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceClaims_InvoiceId",
                table: "InsuranceClaims",
                newName: "IX_InsuranceClaims_VisitId");

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

            migrationBuilder.AddColumn<decimal>(
                name: "ClaimAmount",
                table: "InsuranceClaims",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "InsuranceClaims",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Billings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceCoverage = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    PatientResponsibility = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentMethod = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TransactionId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billings_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Billings_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_BillingId1",
                table: "Visits",
                column: "BillingId1");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_PatientId",
                table: "InsuranceClaims",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Billings_PatientId",
                table: "Billings",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Billings_VisitId",
                table: "Billings",
                column: "VisitId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceClaims_Patients_PatientId",
                table: "InsuranceClaims",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceClaims_Visits_VisitId",
                table: "InsuranceClaims",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Billings_BillingId1",
                table: "Visits",
                column: "BillingId1",
                principalTable: "Billings",
                principalColumn: "Id");
        }
    }
}
