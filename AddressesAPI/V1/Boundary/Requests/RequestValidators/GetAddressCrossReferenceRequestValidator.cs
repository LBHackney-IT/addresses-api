using FluentValidation;

namespace AddressesAPI.V1.Boundary.Requests
{
    public class GetAddressCrossReferenceRequestValidator : AbstractValidator<GetAddressCrossReferenceRequest>
    {
        public GetAddressCrossReferenceRequestValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.uprn).NotNull().NotEmpty().WithMessage("UPRN must be provided");
        }
    }
}
