using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Advertising.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdvertisingInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "advertising");

            migrationBuilder.CreateTable(
                name: "Campaigns",
                schema: "advertising",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StoppedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ProductId",
                schema: "advertising",
                table: "Campaigns",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_SellerId",
                schema: "advertising",
                table: "Campaigns",
                column: "SellerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Campaigns",
                schema: "advertising");
        }
    }
}
