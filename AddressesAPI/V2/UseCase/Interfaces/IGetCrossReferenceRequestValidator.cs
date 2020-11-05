using AddressesAPI.V2.Boundary.Requests;
using FluentValidation.Results;

namespace AddressesAPI.V2.UseCase.Interfaces
{
    public interface IGetCrossReferenceRequestValidator
    {
        ValidationResult Validate(GetAddressCrossReferenceRequest instance);
    }
}
