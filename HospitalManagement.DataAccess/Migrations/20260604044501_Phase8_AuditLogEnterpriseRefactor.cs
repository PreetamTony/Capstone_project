using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Phase8_AuditLogEnterpriseRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TableName",
                table: "AuditLogs",
                newName: "EntityName");

            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ALTER COLUMN \"OldValues\" TYPE jsonb USING \"OldValues\"::jsonb;");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ALTER COLUMN \"NewValues\" TYPE jsonb USING \"NewValues\"::jsonb;");

            migrationBuilder.AddColumn<string[]>(
                name: "ChangedFields",
                table: "AuditLogs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "AuditLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PerformedByName",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserRole",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CorrelationId",
                table: "AuditLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName",
                table: "AuditLogs",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CorrelationId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ChangedFields",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "PerformedByName",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserRole",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "AuditLogs",
                newName: "TableName");

            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ALTER COLUMN \"OldValues\" TYPE text USING \"OldValues\"::text;");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ALTER COLUMN \"NewValues\" TYPE text USING \"NewValues\"::text;");
        }
    }
}
