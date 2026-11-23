using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelFlow.Modules.Shipments.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261123165544_AddOutboxAndConcurrency : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            schema: "shipments",
            table: "Shipments",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            schema: "shipments",
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

        migrationBuilder.CreateIndex(
            name: "IX_OutboxMessages_OccurredOnUtc",
            schema: "shipments",
            table: "OutboxMessages",
            column: "OccurredOnUtc",
            filter: "\"ProcessedOnUtc\" IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OutboxMessages",
            schema: "shipments");

        migrationBuilder.DropColumn(
            name: "xmin",
            schema: "shipments",
            table: "Shipments");
    }
}
