using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressCountryCode",
                schema: "identity",
                table: "WholesalerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressDistrictCode",
                schema: "identity",
                table: "WholesalerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "identity",
                table: "WholesalerProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                schema: "identity",
                table: "WholesalerProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLocality",
                schema: "identity",
                table: "WholesalerProfiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressPostalCode",
                schema: "identity",
                table: "WholesalerProfiles",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressRegionCode",
                schema: "identity",
                table: "WholesalerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCountryCode",
                schema: "identity",
                table: "SellerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressDistrictCode",
                schema: "identity",
                table: "SellerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "identity",
                table: "SellerProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                schema: "identity",
                table: "SellerProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLocality",
                schema: "identity",
                table: "SellerProfiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressPostalCode",
                schema: "identity",
                table: "SellerProfiles",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressRegionCode",
                schema: "identity",
                table: "SellerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCountryCode",
                schema: "identity",
                table: "BuyerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressDistrictCode",
                schema: "identity",
                table: "BuyerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "identity",
                table: "BuyerProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                schema: "identity",
                table: "BuyerProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLocality",
                schema: "identity",
                table: "BuyerProfiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressPostalCode",
                schema: "identity",
                table: "BuyerProfiles",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressRegionCode",
                schema: "identity",
                table: "BuyerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "identity",
                table: "BuyerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCountryCode",
                schema: "identity",
                table: "WholesalerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressDistrictCode",
                schema: "identity",
                table: "WholesalerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "identity",
                table: "WholesalerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                schema: "identity",
                table: "WholesalerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLocality",
                schema: "identity",
                table: "WholesalerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressPostalCode",
                schema: "identity",
                table: "WholesalerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressRegionCode",
                schema: "identity",
                table: "WholesalerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressCountryCode",
                schema: "identity",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressDistrictCode",
                schema: "identity",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "identity",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                schema: "identity",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLocality",
                schema: "identity",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressPostalCode",
                schema: "identity",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressRegionCode",
                schema: "identity",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressCountryCode",
                schema: "identity",
                table: "BuyerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressDistrictCode",
                schema: "identity",
                table: "BuyerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "identity",
                table: "BuyerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                schema: "identity",
                table: "BuyerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLocality",
                schema: "identity",
                table: "BuyerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressPostalCode",
                schema: "identity",
                table: "BuyerProfiles");

            migrationBuilder.DropColumn(
                name: "AddressRegionCode",
                schema: "identity",
                table: "BuyerProfiles");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "identity",
                table: "BuyerProfiles");
        }
    }
}
