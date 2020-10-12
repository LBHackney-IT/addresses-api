using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;

namespace AddressesAPI.V1.UseCase.Interfaces
{
    public interface ISearchAddressUseCase
    {
        SearchAddressResponse ExecuteAsync(SearchAddressRequest request);
    }
}
