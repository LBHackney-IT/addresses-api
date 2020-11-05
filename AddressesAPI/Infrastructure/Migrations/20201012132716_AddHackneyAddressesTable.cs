using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressesAPI.Infrastructure.Migrations
{
    public partial class AddHackneyAddressesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hackney_address",
                schema: "dbo",
                columns: table => new
                {
                    lpi_key = table.Column<string>(maxLength: 14, nullable: false),
                    lpi_logical_status = table.Column<string>(maxLength: 18, nullable: true),
                    lpi_start_date = table.Column<int>(nullable: false),
                    lpi_end_date = table.Column<int>(nullable: false),
                    lpi_last_update_date = table.Column<int>(nullable: false),
                    usrn = table.Column<int>(nullable: true),
                    uprn = table.Column<long>(nullable: false),
                    parent_uprn = table.Column<long>(nullable: true),
                    blpu_start_date = table.Column<int>(nullable: false),
                    blpu_end_date = table.Column<int>(nullable: false),
                    blpu_class = table.Column<string>(maxLength: 4, nullable: true),
                    blpu_last_update_date = table.Column<int>(nullable: false),
                    usage_description = table.Column<string>(maxLength: 160, nullable: true),
                    usage_primary = table.Column<string>(maxLength: 160, nullable: true),
                    property_shell = table.Column<bool>(nullable: false),
                    easting = table.Column<double>(nullable: false),
                    northing = table.Column<double>(nullable: false),
                    unit_number = table.Column<string>(maxLength: 17, nullable: true),
                    sao_text = table.Column<string>(maxLength: 90, nullable: true),
                    building_number = table.Column<string>(maxLength: 17, nullable: true),
                    pao_text = table.Column<string>(maxLength: 90, nullable: true),
                    paon_start_num = table.Column<short>(nullable: true),
                    street_description = table.Column<string>(maxLength: 100, nullable: true),
                    locality = table.Column<string>(maxLength: 100, nullable: true),
                    ward = table.Column<string>(maxLength: 100, nullable: true),
                    town = table.Column<string>(maxLength: 100, nullable: true),
                    postcode = table.Column<string>(maxLength: 8, nullable: true),
                    postcode_nospace = table.Column<string>(maxLength: 8, nullable: true),
                    planning_use_class = table.Column<string>(maxLength: 50, nullable: true),
                    neverexport = table.Column<bool>(nullable: false),
                    longitude = table.Column<double>(nullable: false),
                    latitude = table.Column<double>(nullable: false),
                    gazetteer = table.Column<string>(maxLength: 8, nullable: true),
                    organisation = table.Column<string>(maxLength: 100, nullable: true),
                    line1 = table.Column<string>(maxLength: 200, nullable: true),
                    line2 = table.Column<string>(maxLength: 200, nullable: true),
                    line3 = table.Column<string>(maxLength: 200, nullable: true),
                    line4 = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hackney_address", x => x.lpi_key);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hackney_address",
                schema: "dbo");
        }
    }
}
