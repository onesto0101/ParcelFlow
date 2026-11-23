using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelFlow.Modules.Tracking.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261123165629_InitialTracking : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "tracking");

        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            schema: "tracking",
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
            name: "ShipmentTrackings",
            schema: "tracking",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                TrackingNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CurrentStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                CurrentLocation = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipmentTrackings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TrackingEvents",
            schema: "tracking",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ShipmentTrackingId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TrackingEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_TrackingEvents_ShipmentTrackings_ShipmentTrackingId",
                    column: x => x.ShipmentTrackingId,
                    principalSchema: "tracking",
                    principalTable: "ShipmentTrackings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OutboxMessages_OccurredOnUtc",
            schema: "tracking",
            table: "OutboxMessages",
            column: "OccurredOnUtc",
            filter: "\"ProcessedOnUtc\" IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentTrackings_ShipmentId",
            schema: "tracking",
            table: "ShipmentTrackings",
            column: "ShipmentId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ShipmentTrackings_TrackingNumber",
            schema: "tracking",
            table: "ShipmentTrackings",
            column: "TrackingNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TrackingEvents_ShipmentTrackingId_OccurredAt",
            schema: "tracking",
            table: "TrackingEvents",
            columns: new[] { "ShipmentTrackingId", "OccurredAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OutboxMessages",
            schema: "tracking");

        migrationBuilder.DropTable(
            name: "TrackingEvents",
            schema: "tracking");

        migrationBuilder.DropTable(
            name: "ShipmentTrackings",
            schema: "tracking");
    }
}
