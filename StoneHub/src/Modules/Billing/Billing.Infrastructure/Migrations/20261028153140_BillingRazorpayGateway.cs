using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BillingRazorpayGateway : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CheckoutExpiresAt",
                schema: "billing",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                schema: "billing",
                table: "Subscriptions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewaySubscriptionId",
                schema: "billing",
                table: "Subscriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WillRenew",
                schema: "billing",
                table: "Subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ProcessedPaymentEvents",
                schema: "billing",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedPaymentEvents", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_GatewaySubscriptionId",
                schema: "billing",
                table: "Subscriptions",
                column: "GatewaySubscriptionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedPaymentEvents",
                schema: "billing");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_GatewaySubscriptionId",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CheckoutExpiresAt",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "GatewaySubscriptionId",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "WillRenew",
                schema: "billing",
                table: "Subscriptions");
        }
    }
}
