using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressesAPI.Infrastructure.Migrations
{
    [Migration("20260706142000_RemoveUsagePrimaryIndexes")]
    public partial class RemoveUsagePrimaryIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS dbo.hackney_address_usage_primary_idx;",
                suppressTransaction: true);
            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS dbo.national_address_usage_primary_idx;",
                suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS hackney_address_usage_primary_idx ON dbo.hackney_address USING btree (usage_primary);",
                suppressTransaction: true);
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS national_address_usage_primary_idx ON dbo.national_address USING btree (usage_primary);",
                suppressTransaction: true);
        }
    }
}
