using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressesAPI.V1.Infrastructure.Migrations
{
    public partial class CreateCombinedAddressesView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE VIEW ""dbo.combined_address"" AS SELECT * FROM dbo.hackney_address UNION ALL SELECT * FROM dbo.national_address;");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW ""dbo.combined_address"";");

        }
    }
}
