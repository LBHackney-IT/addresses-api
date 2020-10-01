using FluentValidation;

namespace AddressesAPI.V1.Boundary.Requests.RequestValidators
{
    public class SearchAddressRequestValidator : AbstractValidator<SearchAddressRequest>
    {
        public SearchAddressRequestValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.PageSize).LessThan(51).WithMessage("PageSize cannot exceed 50");
            //RuleFor(x => x.addressID).NotNull().NotEmpty().WithMessage("addressID must be provided");
            //RuleFor(x => x.addressID).Length(14).WithMessage("addressID must be 14 characters");

        }
    }
}
