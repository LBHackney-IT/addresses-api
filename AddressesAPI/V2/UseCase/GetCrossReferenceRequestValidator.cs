using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.UseCase.Interfaces;
using FluentValidation;

namespace AddressesAPI.V2.UseCase
{
    public class GetCrossReferenceRequestValidator : AbstractValidator<GetAddressCrossReferenceRequest>, IGetCrossReferenceRequestValidator
    {
        public GetCrossReferenceRequestValidator()
        {
            RuleFor(x => x).NotNull().WithMessage("request is null");
            RuleFor(x => x.uprn).NotNull().NotEmpty().WithMessage("UPRN must be provided");
        }
    }
}
