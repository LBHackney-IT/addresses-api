using System.Collections.Generic;
using LBHAddressesAPI.Infrastructure.V1.Validation;

namespace AddressesAPI.V1.Boundary.Responses.Metadata
{
    public class APIError
    {
        public bool IsValid { get; set; }
        public IList<ValidationError> ValidationErrors { get; set; }
    }
}
