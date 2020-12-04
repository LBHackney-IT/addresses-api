using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.UseCase.Interfaces;
using FluentValidation;

namespace AddressesAPI.V2.UseCase
{
    public class SearchAddressValidator : AbstractValidator<SearchAddressRequest>, ISearchAddressValidator
    {
        public SearchAddressValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(r => r.AddressStatus).NotNull().NotEmpty();
            RuleFor(r => r.AddressStatus)
                .Must(CanBeAnyCombinationOfAllowedAddressStatuses)
                .WithMessage("Value for the parameter is not valid.");

            RuleFor(r => r.AddressScope)
                .Must(addressScope => Enum.TryParse<GlobalConstants.AddressScope>(addressScope?.Replace(" ", ""), true, out _))
                .WithMessage("Value for the parameter is not valid. It should be either Hackney Borough, Hackney Gazetteer or National.");

            RuleFor(r => r.Format)
                .Must(format => Enum.TryParse<GlobalConstants.Format>(format, true, out _))
                .WithMessage("Value for Format is not valid. It should be either Simple or Detailed");

            RuleFor((r => r.Page)).GreaterThan(0)
                .WithMessage("Invalid page value. Page number can not be a negative value");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Invalid Page Size value. Page Size can not be a negative value");

            RuleFor(x => x.PageSize)
                .LessThan(51).WithMessage("PageSize cannot exceed 50");

            RuleFor(r => r.Postcode)
                .Matches(new Regex("^((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]))))( )?(([0-9][A-Za-z]?[A-Za-z]?)?))$"))
                .WithMessage("Must provide at least the first part of the postcode.");

            RuleFor(r => r.ModifiedSince)
                .Must(modifiedSince => modifiedSince == null
                        || DateTime.TryParseExact(modifiedSince, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                .WithMessage("Invalid date format. Please provide date in the format YYYY-MM-DD");

            RuleFor(r => r)
                .Must(CheckForAtLeastOneMandatoryFilterPropertyWithHackneyGazetteer)
                .WithMessage("You must provide at least one of (query, uprn, usrn, postcode, street, usagePrimary, usageCode), when address_scope is 'hackney borough' or 'hackney gazetteer'.");

            RuleFor(r => r)
                .Must(CheckForAtLeastOneMandatoryFilterPropertyWithNationalGazetteer)
                .WithMessage("You must provide at least one of (query, uprn, usrn, postcode), when address_scope is 'national'.");

            RuleFor(r => r)
                .Must(CheckCrossReferenceCodeWhenGivenHasAValue)
                .WithMessage("You must provide both the code and a value, when searching by a cross reference");

            RuleFor(r => r.RequestFields)
                .Must(CheckForInvalidProperties)
                .WithMessage("Invalid properties have been provided.");
        }

        private static bool CheckForInvalidProperties(List<string> requestFields)
        {
            if (requestFields == null) //When the api runs - this will never be null. However this will become null in the context of other validation tests, making them crash. Because fluent validations testshelper doesn't isolate different rules one from another.
            {
                return true; // returning true, because there can't be any invalid parameter names, when there no parameters provided.
            }

            var removeQueryUnderscores = requestFields.Select(x => x.Replace("_", "")).ToList();

            var allProperties = typeof(SearchAddressRequest).GetProperties().Where(prop => prop.Name != "Errors" && prop.Name != "RequestFields").Select(prop => prop.Name).ToList();

            var invalidParameters = removeQueryUnderscores.Except(allProperties, StringComparer.OrdinalIgnoreCase);

            return !invalidParameters.Any();
        }

        private static bool CheckForAtLeastOneMandatoryFilterPropertyWithHackneyGazetteer(SearchAddressRequest request)
        {
            var crossReferenceParametersGiven = request.CrossRefCode != null && request.CrossRefValue != null;

            if (crossReferenceParametersGiven)
            {
                return true;
            }

            return request.AddressScope.Equals(GlobalConstants.AddressScope.National.ToString(), StringComparison.InvariantCultureIgnoreCase)
                  || request.UPRN != null
                  || request.USRN != null
                  || request.Postcode != null
                  || request.Street != null
                  || request.UsagePrimary != null
                  || request.UsageCode != null
                  || !string.IsNullOrWhiteSpace(request.Query);
        }

        private static bool CheckForAtLeastOneMandatoryFilterPropertyWithNationalGazetteer(SearchAddressRequest request)
        {
            return !request.AddressScope.Equals(GlobalConstants.AddressScope.National.ToString(), StringComparison.InvariantCultureIgnoreCase)
                   || request.UPRN != null
                   || request.USRN != null
                   || request.Postcode != null
                   || !string.IsNullOrWhiteSpace(request.Query);
        }

        private static bool CheckCrossReferenceCodeWhenGivenHasAValue(SearchAddressRequest request)
        {
            var bothAreNull = string.IsNullOrWhiteSpace(request.CrossRefCode) && string.IsNullOrWhiteSpace(request.CrossRefValue);
            var bothAreGiven = request.CrossRefCode != null && request.CrossRefValue != null;

            return bothAreNull || bothAreGiven;
        }

        private static bool CanBeAnyCombinationOfAllowedAddressStatuses(string addressStatus)
        {
            var allowedValues = new List<string> { "historical", "alternative", "approved", "provisional" };
            if (string.IsNullOrEmpty(addressStatus))
            {
                return false;
            }
            var separateValuesArray = addressStatus.Split(",");

            return separateValuesArray.All(value => allowedValues.Contains(value.ToLower()));
        }
    }
}
