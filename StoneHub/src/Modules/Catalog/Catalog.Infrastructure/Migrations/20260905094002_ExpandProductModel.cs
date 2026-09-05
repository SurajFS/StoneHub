using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Optional spec fields — not every category (e.g. Handicrafts) has size/thickness/finish.
            migrationBuilder.AlterColumn<string>(
                name: "Thickness", schema: "catalog", table: "Products",
                type: "character varying(50)", maxLength: 50, nullable: true,
                oldClrType: typeof(string), oldType: "character varying(50)", oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Size", schema: "catalog", table: "Products",
                type: "character varying(100)", maxLength: 100, nullable: true,
                oldClrType: typeof(string), oldType: "character varying(100)", oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Finish", schema: "catalog", table: "Products",
                type: "character varying(50)", maxLength: 50, nullable: true,
                oldClrType: typeof(string), oldType: "character varying(50)", oldMaxLength: 50);

            // New columns. The required ones go in nullable first so existing rows can be
            // backfilled from MaterialType before NOT NULL is enforced.
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId", schema: "catalog", table: "Products", type: "uuid", nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "SubcategoryId", schema: "catalog", table: "Products", type: "uuid", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Color", schema: "catalog", table: "Products",
                type: "character varying(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "OwnerType", schema: "catalog", table: "Products",
                type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Unit", schema: "catalog", table: "Products",
                type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<List<string>>(
                name: "Tags", schema: "catalog", table: "Products", type: "text[]", nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsActive", schema: "catalog", table: "Products", type: "boolean", nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "WholesalePrice", schema: "catalog", table: "Products", type: "numeric(18,2)", nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "MinimumOrderQuantity", schema: "catalog", table: "Products", type: "numeric(18,2)", nullable: true);

            // Backfill existing rows from the retiring MaterialType (no-op on a fresh database).
            migrationBuilder.Sql(
                """
                UPDATE catalog."Products"
                SET "CategoryId" = CASE "MaterialType"
                        WHEN 'Granite' THEN '11111111-1111-1111-1111-000000000002'::uuid
                        ELSE '11111111-1111-1111-1111-000000000001'::uuid
                    END,
                    "OwnerType" = 'Seller',
                    "Unit" = 'Slab',
                    "Tags" = ARRAY[]::text[],
                    "IsActive" = true
                WHERE "CategoryId" IS NULL;
                """);

            // Enforce NOT NULL now that every row has values.
            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId", schema: "catalog", table: "Products",
                type: "uuid", nullable: false, oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "OwnerType", schema: "catalog", table: "Products",
                type: "character varying(20)", maxLength: 20, nullable: false,
                oldClrType: typeof(string), oldType: "character varying(20)", oldMaxLength: 20, oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Unit", schema: "catalog", table: "Products",
                type: "character varying(20)", maxLength: 20, nullable: false,
                oldClrType: typeof(string), oldType: "character varying(20)", oldMaxLength: 20, oldNullable: true);
            migrationBuilder.AlterColumn<List<string>>(
                name: "Tags", schema: "catalog", table: "Products",
                type: "text[]", nullable: false,
                oldClrType: typeof(List<string>), oldType: "text[]", oldNullable: true);
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive", schema: "catalog", table: "Products",
                type: "boolean", nullable: false, oldClrType: typeof(bool), oldType: "boolean", oldNullable: true);

            // Retire MaterialType.
            migrationBuilder.DropIndex(name: "IX_Products_MaterialType", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "MaterialType", schema: "catalog", table: "Products");

            // Indexes for the new filterable columns.
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId", schema: "catalog", table: "Products", column: "CategoryId");
            migrationBuilder.CreateIndex(
                name: "IX_Products_SubcategoryId", schema: "catalog", table: "Products", column: "SubcategoryId");
            migrationBuilder.CreateIndex(
                name: "IX_Products_OwnerType", schema: "catalog", table: "Products", column: "OwnerType");
            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive", schema: "catalog", table: "Products", column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Products_CategoryId", schema: "catalog", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_IsActive", schema: "catalog", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_OwnerType", schema: "catalog", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_SubcategoryId", schema: "catalog", table: "Products");

            migrationBuilder.DropColumn(name: "CategoryId", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "Color", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "IsActive", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "MinimumOrderQuantity", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "OwnerType", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "SubcategoryId", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "Tags", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "Unit", schema: "catalog", table: "Products");
            migrationBuilder.DropColumn(name: "WholesalePrice", schema: "catalog", table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Thickness", schema: "catalog", table: "Products",
                type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "",
                oldClrType: typeof(string), oldType: "character varying(50)", oldMaxLength: 50, oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Size", schema: "catalog", table: "Products",
                type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "",
                oldClrType: typeof(string), oldType: "character varying(100)", oldMaxLength: 100, oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Finish", schema: "catalog", table: "Products",
                type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "",
                oldClrType: typeof(string), oldType: "character varying(50)", oldMaxLength: 50, oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialType", schema: "catalog", table: "Products",
                type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Products_MaterialType", schema: "catalog", table: "Products", column: "MaterialType");
        }
    }
}
