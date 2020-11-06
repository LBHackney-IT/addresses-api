
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;

namespace AddressesAPI.V2.UseCase.Interfaces
{
    public interface ISearchAddressUseCase
    {
        SearchAddressResponse ExecuteAsync(SearchAddressRequest request);
    }
}
