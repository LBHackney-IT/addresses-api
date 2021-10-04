using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressesAPI.Infrastructure.Migrations
{
    public partial class AddCombinedAddressesViewWithDboSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DROP VIEW ""dbo.combined_address"";
                CREATE VIEW dbo.combined_address AS SELECT hackney_address.lpi_key,
                    hackney_address.lpi_logical_status,
                    hackney_address.usrn,
                    hackney_address.uprn,
                    hackney_address.parent_uprn,
                    hackney_address.blpu_start_date,
                    hackney_address.blpu_end_date,
                    hackney_address.blpu_class,
                    hackney_address.blpu_last_update_date,
                    hackney_address.usage_description,
                    hackney_address.usage_primary,
                    hackney_address.property_shell,
                    hackney_address.easting,
                    hackney_address.northing,
                    hackney_address.unit_number as unit_number,
                    hackney_address.sao_text,
                    hackney_address.building_number,
                    hackney_address.pao_text,
                    hackney_address.paon_start_num,
                    hackney_address.street_description,
                    hackney_address.locality,
                    hackney_address.ward,
                    hackney_address.town,
                    hackney_address.postcode,
                    hackney_address.postcode_nospace,
                    hackney_address.planning_use_class,
                    hackney_address.neverexport,
                    hackney_address.longitude,
                    hackney_address.latitude,
                    hackney_address.gazetteer,
                    hackney_address.organisation,
                    hackney_address.line1,
                    hackney_address.line2,
                    hackney_address.line3,
                    hackney_address.line4
                   FROM dbo.hackney_address UNION ALL
                SELECT national_address.lpi_key,
                    national_address.lpi_logical_status,
                    national_address.usrn,
                    national_address.uprn,
                    national_address.parent_uprn,
                    national_address.blpu_start_date,
                    national_address.blpu_end_date,
                    national_address.blpu_class,
                    national_address.blpu_last_update_date,
                    national_address.usage_description,
                    national_address.usage_primary,
                    national_address.property_shell,
                    national_address.easting,
                    national_address.northing,
                    national_address.unit_number,
                    national_address.sao_text,
                    national_address.building_number,
                    national_address.pao_text,
                    national_address.paon_start_num,
                    national_address.street_description,
                    national_address.locality,
                    national_address.ward,
                    national_address.town,
                    national_address.postcode,
                    national_address.postcode_nospace,
                    national_address.planning_use_class,
                    national_address.neverexport,
                    national_address.longitude,
                    national_address.latitude,
                    national_address.gazetteer,
                    national_address.organisation,
                    national_address.line1,
                    national_address.line2,
                    national_address.line3,
                    national_address.line4
                   FROM dbo.national_address;"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DROP VIEW dbo.combined_address;
                CREATE VIEW ""dbo.combined_address"" AS SELECT * FROM dbo.hackney_address UNION ALL SELECT * FROM dbo.national_address;"
                );
        }
    }
}
