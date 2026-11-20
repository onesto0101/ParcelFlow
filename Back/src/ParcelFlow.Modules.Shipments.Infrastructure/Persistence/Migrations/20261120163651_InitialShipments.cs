using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelFlow.Modules.Shipments.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261120163651_InitialShipments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "shipments");

        migrationBuilder.CreateTable(
            name: "Shipments",
            schema: "shipments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TrackingNumber = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Cost_Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                Cost_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                Dimensions_Height = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                Dimensions_Length = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                Dimensions_Width = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                Recipient_Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Recipient_Phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                Recipient_Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Recipient_Address_Country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                Recipient_Address_PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Recipient_Address_State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Recipient_Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Sender_Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Sender_Phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                Sender_Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Sender_Address_Country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                Sender_Address_PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Sender_Address_State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Sender_Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Weight_Kilograms = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Shipments", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Shipments_TrackingNumber",
            schema: "shipments",
            table: "Shipments",
            column: "TrackingNumber",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Shipments",
            schema: "shipments");
    }
}
