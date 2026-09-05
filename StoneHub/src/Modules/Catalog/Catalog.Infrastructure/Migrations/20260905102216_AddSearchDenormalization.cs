using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchDenormalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerLocation",
                schema: "catalog",
                table: "Products",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerName",
                schema: "catalog",
                table: "Products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // Trigram-based partial/fuzzy matching for keyword, seller-name and location search.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Products_Title_trgm"" ON catalog.""Products"" USING gin (""Title"" gin_trgm_ops);");
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Products_SellerName_trgm"" ON catalog.""Products"" USING gin (""SellerName"" gin_trgm_ops);");
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Products_SellerLocation_trgm"" ON catalog.""Products"" USING gin (""SellerLocation"" gin_trgm_ops);");
            // Array GIN index for tag containment / keyword-in-tags lookups.
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Products_Tags_gin"" ON catalog.""Products"" USING gin (""Tags"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Indexes on retained columns must be dropped explicitly; the SellerName/SellerLocation
            // trigram indexes drop automatically with their columns below.
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS catalog.""IX_Products_Title_trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS catalog.""IX_Products_Tags_gin"";");

            migrationBuilder.DropColumn(
                name: "SellerLocation",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellerName",
                schema: "catalog",
                table: "Products");
        }
    }
}
