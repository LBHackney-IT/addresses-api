
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;

namespace AddressesAPI.V2.UseCase.Interfaces
{
    public interface IGetSingleAddressUseCase
    {
        GetAddressResponse ExecuteAsync(GetAddressRequest request);
    }
}
