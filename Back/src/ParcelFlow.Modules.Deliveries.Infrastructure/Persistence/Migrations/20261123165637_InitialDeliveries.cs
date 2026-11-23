using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelFlow.Modules.Deliveries.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261123165637_InitialDeliveries : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "deliveries");

        migrationBuilder.CreateTable(
            name: "Deliveries",
            schema: "deliveries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                TrackingNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CourierId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Deliveries", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            schema: "deliveries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Attempts = table.Column<int>(type: "integer", nullable: false),
                LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OutboxMessages", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "DeliveryAttempts",
            schema: "deliveries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                CourierId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                AttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                DeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                Evidence_Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                Evidence_ReceivedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                FailureReason_Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                FailureReason_Details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DeliveryAttempts", x => x.Id);
                table.ForeignKey(
                    name: "FK_DeliveryAttempts_Deliveries_DeliveryId",
                    column: x => x.DeliveryId,
                    principalSchema: "deliveries",
                    principalTable: "Deliveries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Deliveries_CourierId",
            schema: "deliveries",
            table: "Deliveries",
            column: "CourierId");

        migrationBuilder.CreateIndex(
            name: "IX_Deliveries_ShipmentId",
            schema: "deliveries",
            table: "Deliveries",
            column: "ShipmentId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Deliveries_Status",
            schema: "deliveries",
            table: "Deliveries",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Deliveries_TrackingNumber",
            schema: "deliveries",
            table: "Deliveries",
            column: "TrackingNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DeliveryAttempts_DeliveryId",
            schema: "deliveries",
            table: "DeliveryAttempts",
            column: "DeliveryId");

        migrationBuilder.CreateIndex(
            name: "IX_OutboxMessages_OccurredOnUtc",
            schema: "deliveries",
            table: "OutboxMessages",
            column: "OccurredOnUtc",
            filter: "\"ProcessedOnUtc\" IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DeliveryAttempts",
            schema: "deliveries");

        migrationBuilder.DropTable(
            name: "OutboxMessages",
            schema: "deliveries");

        migrationBuilder.DropTable(
            name: "Deliveries",
            schema: "deliveries");
    }
}
