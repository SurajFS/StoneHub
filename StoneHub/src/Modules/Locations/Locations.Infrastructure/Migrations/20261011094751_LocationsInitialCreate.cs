using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Locations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocationsInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "locations");

            migrationBuilder.CreateTable(
                name: "Countries",
                schema: "locations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HasRegions = table.Column<bool>(type: "boolean", nullable: false),
                    PostalCodePattern = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatasetImports",
                schema: "locations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ImportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatasetImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                schema: "locations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    CountryId = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "locations",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                schema: "locations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RegionId = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Regions_RegionId",
                        column: x => x.RegionId,
                        principalSchema: "locations",
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostalLocalities",
                schema: "locations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    CountryId = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    DistrictId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalLocalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostalLocalities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "locations",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostalLocalities_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "locations",
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "locations",
                table: "Countries",
                columns: new[] { "Id", "HasRegions", "Name", "PostalCodePattern" },
                values: new object[] { "IN", true, "India", "^[1-9][0-9]{5}$" });

            migrationBuilder.InsertData(
                schema: "locations",
                table: "Regions",
                columns: new[] { "Id", "CountryId", "Name" },
                values: new object[,]
                {
                    { "IN-AN", "IN", "Andaman and Nicobar Islands" },
                    { "IN-AP", "IN", "Andhra Pradesh" },
                    { "IN-AR", "IN", "Arunachal Pradesh" },
                    { "IN-AS", "IN", "Assam" },
                    { "IN-BR", "IN", "Bihar" },
                    { "IN-CG", "IN", "Chhattisgarh" },
                    { "IN-CH", "IN", "Chandigarh" },
                    { "IN-DH", "IN", "Dadra and Nagar Haveli and Daman and Diu" },
                    { "IN-DL", "IN", "Delhi" },
                    { "IN-GA", "IN", "Goa" },
                    { "IN-GJ", "IN", "Gujarat" },
                    { "IN-HP", "IN", "Himachal Pradesh" },
                    { "IN-HR", "IN", "Haryana" },
                    { "IN-JH", "IN", "Jharkhand" },
                    { "IN-JK", "IN", "Jammu and Kashmir" },
                    { "IN-KA", "IN", "Karnataka" },
                    { "IN-KL", "IN", "Kerala" },
                    { "IN-LA", "IN", "Ladakh" },
                    { "IN-LD", "IN", "Lakshadweep" },
                    { "IN-MH", "IN", "Maharashtra" },
                    { "IN-ML", "IN", "Meghalaya" },
                    { "IN-MN", "IN", "Manipur" },
                    { "IN-MP", "IN", "Madhya Pradesh" },
                    { "IN-MZ", "IN", "Mizoram" },
                    { "IN-NL", "IN", "Nagaland" },
                    { "IN-OD", "IN", "Odisha" },
                    { "IN-PB", "IN", "Punjab" },
                    { "IN-PY", "IN", "Puducherry" },
                    { "IN-RJ", "IN", "Rajasthan" },
                    { "IN-SK", "IN", "Sikkim" },
                    { "IN-TN", "IN", "Tamil Nadu" },
                    { "IN-TR", "IN", "Tripura" },
                    { "IN-TS", "IN", "Telangana" },
                    { "IN-UK", "IN", "Uttarakhand" },
                    { "IN-UP", "IN", "Uttar Pradesh" },
                    { "IN-WB", "IN", "West Bengal" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Districts_RegionId",
                schema: "locations",
                table: "Districts",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_PostalLocalities_CountryId_PostalCode_DistrictId_Name",
                schema: "locations",
                table: "PostalLocalities",
                columns: new[] { "CountryId", "PostalCode", "DistrictId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostalLocalities_DistrictId",
                schema: "locations",
                table: "PostalLocalities",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CountryId",
                schema: "locations",
                table: "Regions",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatasetImports",
                schema: "locations");

            migrationBuilder.DropTable(
                name: "PostalLocalities",
                schema: "locations");

            migrationBuilder.DropTable(
                name: "Districts",
                schema: "locations");

            migrationBuilder.DropTable(
                name: "Regions",
                schema: "locations");

            migrationBuilder.DropTable(
                name: "Countries",
                schema: "locations");
        }
    }
}
