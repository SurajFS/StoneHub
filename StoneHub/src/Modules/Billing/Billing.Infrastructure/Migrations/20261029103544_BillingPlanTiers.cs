using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BillingPlanTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutGatewaySubscriptionId",
                schema: "billing",
                table: "Subscriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutPlan",
                schema: "billing",
                table: "Subscriptions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasEverPaid",
                schema: "billing",
                table: "Subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CheckoutGatewaySubscriptionId",
                schema: "billing",
                table: "Subscriptions",
                column: "CheckoutGatewaySubscriptionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_CheckoutGatewaySubscriptionId",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CheckoutGatewaySubscriptionId",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CheckoutPlan",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "HasEverPaid",
                schema: "billing",
                table: "Subscriptions");
        }
    }
}
