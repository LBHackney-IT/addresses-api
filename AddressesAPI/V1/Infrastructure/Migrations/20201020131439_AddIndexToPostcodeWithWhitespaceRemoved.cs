using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressesAPI.V1.Infrastructure.Migrations
{
    public partial class AddIndexToPostcodeWithWhitespaceRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS national_address_postcode_trgm_whitespace_removed_idx ON dbo.national_address USING gin (replace((postcode)::text, ' '::text, ''::text) gin_trgm_ops);");
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS hackney_address_postcode_trgm_whitespace_removed_idx ON dbo.hackney_address USING gin (replace((postcode)::text, ' '::text, ''::text) gin_trgm_ops);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.national_address_postcode_trgm_whitespace_removed_idx;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS dbo.hackney_address_postcode_trgm_whitespace_removed_idx;");
        }
    }
}
