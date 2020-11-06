using AddressesAPI.V2.Boundary.Requests;
using FluentValidation.Results;

namespace AddressesAPI.V2.UseCase.Interfaces
{
    public interface IGetAddressRequestValidator
    {
        ValidationResult Validate(GetAddressRequest instance);
    }
}
