using AddressesAPI.V1.Boundary.Requests;
using FluentValidation.Results;

namespace AddressesAPI.V1.UseCase.Interfaces
{
    public interface ISearchAddressValidator
    {
        ValidationResult Validate(SearchAddressRequest instance);
    }
}
