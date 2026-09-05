using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inquiries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InquiriesInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inquiries");

            migrationBuilder.CreateTable(
                name: "Inquiries",
                schema: "inquiries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    WholesalerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SellerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquiries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_ProductId",
                schema: "inquiries",
                table: "Inquiries",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_SellerId",
                schema: "inquiries",
                table: "Inquiries",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_WholesalerId",
                schema: "inquiries",
                table: "Inquiries",
                column: "WholesalerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inquiries",
                schema: "inquiries");
        }
    }
}
