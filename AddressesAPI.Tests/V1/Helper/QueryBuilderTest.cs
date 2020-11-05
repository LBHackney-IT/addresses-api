using System.Threading.Tasks;
using AddressesAPI.V1;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Infrastructure;
using Dapper;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.Helper
{
    public class QueryBuilderTest
    {
        [Test]
        public void GivenValidSearchAddressRequest_WhenCallingQueryBuilderWithSimpleFormat_ThenShouldReturnCorrectQuery()
        {
            var dbArgs = new DynamicParameters();//dynamically add parameters to Dapper query
            var request = new SearchParameters
            {
                Format = GlobalConstants.Format.Simple,
                Postcode = "",
                Gazetteer = GlobalConstants.Gazetteer.Local
            };

            var response = QueryBuilder.GetSearchAddressQuery(request, true, true, false, ref dbArgs);
            response.Replace("  ", " ").Should().Contain("Line1, Line2, Line3, Line4 , Postcode, UPRN , TOWN as Town  ".Replace("  ", " "));
        }

        [Test]
        public void GivenValidSearchAddressRequest_WhenCallingQueryBuilderWithDetailedFormat_ThenShouldReturnCorrectQuery()
        {
            var dbArgs = new DynamicParameters();//dynamically add parameters to Dapper query
            var request = new SearchParameters
            {
                Format = GlobalConstants.Format.Detailed,
                Postcode = ""
            };

            var response = QueryBuilder.GetSearchAddressQuery(request, true, true, false, ref dbArgs);
            response.Replace("  ", " ").Should().Contain("SELECT lpi_key as addressKey, uprn as uprn, usrn as usrn, parent_uprn as parentUPRN, lpi_logical_status as addressStatus, sao_text as unitName, unit_number as unitNumber, pao_text as buildingName, building_number as buildingNumber, street_description as street, postcode as postcode, locality as locality, gazetteer as gazetteer, organisation as commercialOccupier, ward as ward, usage_description as usageDescription, usage_primary as usagePrimary, blpu_class as usageCode, planning_use_class as planningUseClass, property_shell as propertyShell, neverexport as hackneyGazetteerOutOfBoroughAddress, easting as easting, northing as northing, longitude as longitude, latitude as latitude, lpi_start_date as addressStartDate, lpi_end_date as addressEndDate, lpi_last_update_date as addressChangeDate, blpu_start_date as propertyStartDate, blpu_end_date as propertyEndDate, blpu_last_update_date as propertyChangeDate,".Replace("  ", " "));
        }

        [Test]
        public void GivenValidSearchAddressRequest_WhenCallingQueryBuilderWithParentShell_ThenShouldReturnQueryForIncludingParentShells()
        {
            var dbArgs = new DynamicParameters();//dynamically add parameters to Dapper query
            var request = new SearchParameters
            {
                Format = GlobalConstants.Format.Simple,
                Postcode = "RM12PR",
                UsagePrimary = "ParentShell"
            };

            var response = QueryBuilder.GetSearchAddressQuery(request, true, true, false, ref dbArgs);
            response.Replace(" ", "").Should().Contain(";WITH SEED AS (SELECT * FROM dbo.combined_address L".Replace(" ", ""));
        }

        [Test]
        public void GivenValidSearchAddressRequest_WhenCallingQueryBuilderWithisCountQuery_ThenShouldReturnQueryForCount()
        {
            var dbArgs = new DynamicParameters();//dynamically add parameters to Dapper query
            var request = new SearchParameters
            {
                Format = GlobalConstants.Format.Simple,
                Postcode = "RM12PR"
            };

            var response = QueryBuilder.GetSearchAddressQuery(request, true, true, true, ref dbArgs);
            response.Replace("  ", " ").Should().Contain("SELECT COUNT(1)".Replace("  ", " "));
        }

    }
}
