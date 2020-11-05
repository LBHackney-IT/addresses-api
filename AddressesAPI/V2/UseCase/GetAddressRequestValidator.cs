using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentValidation;

namespace AddressesAPI.V1.UseCase
{
    public class GetAddressRequestValidator : AbstractValidator<GetAddressRequest>, IGetAddressRequestValidator
    {
        public GetAddressRequestValidator()
        {
            RuleFor(x => x).NotNull().WithMessage("request is null");
            RuleFor(x => x.addressID).NotNull().NotEmpty().WithMessage("addressID must be provided");
            RuleFor(x => x.addressID).Length(14).WithMessage("addressID must be 14 characters");
        }
    }
}
