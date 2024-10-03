using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentValidation;

namespace AddressesAPI.V1.UseCase
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

            RuleFor(r => r.Gazetteer)
                .Must(gazetteer => gazetteer.ToLower() == "local" || gazetteer.ToLower() == "both")
                .WithMessage("Value for the parameter is not valid. It should be either 'Local' or 'Both'.");

            RuleFor(r => r.Format)
                .Must(format => Enum.TryParse<GlobalConstants.Format>(format, true, out _))
                .WithMessage("Value for Format is not valid. It should be either Simple or Detailed");

            //TODO - this has just been increased for this spike. See comments in AddressesGateway
            RuleFor(x => x.PageSize)
                .LessThan(1001).WithMessage("PageSize cannot exceed 200");

            RuleFor(r => r.PostCode)
                .Matches(new Regex("^((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]))))( )?(([0-9][A-Za-z]?[A-Za-z]?)?))$"))
                .WithMessage("Must provide at least the first part of the postcode.");

            RuleFor(r => r)
                .Must(CheckForAtLeastOneMandatoryFilterPropertyWithGazetteerLocal)
                .WithMessage("You must provide at least one of (uprn, usrn, postcode, street, usagePrimary, usageCode), when gazeteer is 'local'.");

            RuleFor(r => r)
                .Must(CheckForAtLeastOneMandatoryFilterPropertyWithGazetteerBoth)
                .WithMessage("You must provide at least one of (uprn, usrn, postcode), when gazetteer is 'both'.");

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

            var allProperties = typeof(SearchAddressRequest).GetProperties().Where(prop => prop.Name != "Errors" && prop.Name != "RequestFields").Select(prop => prop.Name).ToList();

            var invalidParameters = requestFields.Except(allProperties, StringComparer.OrdinalIgnoreCase);

            return !invalidParameters.Any();
        }

        private static bool CheckForAtLeastOneMandatoryFilterPropertyWithGazetteerLocal(SearchAddressRequest request)
        {
            return request.Gazetteer == GlobalConstants.Gazetteer.Both.ToString()
                   || request.UPRN != null
                   || request.USRN != null
                   || request.PostCode != null
                   || request.Street != null
                   || request.usagePrimary != null
                   || request.usageCode != null;
        }

        private static bool CheckForAtLeastOneMandatoryFilterPropertyWithGazetteerBoth(SearchAddressRequest request)
        {
            return request.Gazetteer != GlobalConstants.Gazetteer.Both.ToString()
                   || request.UPRN != null
                   || request.USRN != null
                   || request.PostCode != null;
        }

        private static bool CanBeAnyCombinationOfAllowedAddressStatuses(string addressStatus)
        {
            var allowedValues = new List<string> { "historical", "alternative", "approved preferred", "provisional" };
            if (string.IsNullOrEmpty(addressStatus))
            {
                return false;
            }
            var separateValuesArray = addressStatus.Split(",");

            return separateValuesArray.All(value => allowedValues.Contains(value.ToLower()));
        }
    }
}
