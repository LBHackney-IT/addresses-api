using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressesAPI.V1.Infrastructure.Migrations
{
    public partial class AddIndexesToTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            //National Address Indexes
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS national_address_lpi_key_unique_idx ON dbo.national_address USING btree (lpi_key);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_lpi_logical_status_idx ON dbo.national_address USING btree (lpi_logical_status);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_uprn_idx ON dbo.national_address USING btree (uprn);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_usrn_idx ON dbo.national_address USING btree (usrn);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_usage_primary_idx ON dbo.national_address USING btree (usage_primary);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_gazetteer_idx ON dbo.national_address USING btree (gazetteer);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_neverexport_idx ON dbo.national_address USING btree (neverexport);");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_postcode_nospace_trgm_idx ON dbo.national_address USING gin (postcode_nospace gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_building_number_trgm_idx ON dbo.national_address USING gin (building_number gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_blpu_class_trgm_idx ON dbo.national_address USING gin (blpu_class gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS national_address_street_description_trgm_whitespace_removed_idx ON dbo.national_address USING gin (replace((street_description)::text, ' '::text, ''::text) gin_trgm_ops);");

            //Hackney Address Indexes
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS hackney_address_lpi_key_unique_idx ON dbo.hackney_address USING btree (lpi_key);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_lpi_logical_status_idx ON dbo.hackney_address USING btree (lpi_logical_status);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_uprn_idx ON dbo.hackney_address USING btree (uprn);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_usrn_idx ON dbo.hackney_address USING btree (usrn);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_usage_primary_idx ON dbo.hackney_address USING btree (usage_primary);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_gazetteer_idx ON dbo.hackney_address USING btree (gazetteer);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_neverexport_idx ON dbo.hackney_address USING btree (neverexport);");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_postcode_nospace_trgm_idx ON dbo.hackney_address USING gin (postcode_nospace gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_building_number_trgm_idx ON dbo.hackney_address USING gin (building_number gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_blpu_class_trgm_idx ON dbo.hackney_address USING gin (blpu_class gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS hackney_address_street_description_trgm_whitespace_removed_idx ON dbo.hackney_address USING gin (replace((street_description)::text, ' '::text, ''::text) gin_trgm_ops);");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //National Address Indexes
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_postcode_nospace_trgm_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_building_number_trgm_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_blpu_class_trgm_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_street_description_trgm_whitespace_removed_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_lpi_logical_status_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_uprn_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_usrn_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_usage_primary_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_gazetteer_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_neverexport_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_lpi_key_unique_idx;");

            //Hackney Address Indexes
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_postcode_nospace_trgm_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_building_number_trgm_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_blpu_class_trgm_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_street_description_trgm_whitespace_removed_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_lpi_logical_status_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_uprn_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_usrn_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_usage_primary_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_gazetteer_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_neverexport_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_lpi_key_unique_idx;");
        }
    }
}
