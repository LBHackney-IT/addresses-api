using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V1.Controllers
{
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("api/v1/addresses")]
    public class GetAddressController : BaseController
    {
        private readonly IGetSingleAddressUseCase _getAddressUseCase;

        public GetAddressController(IGetSingleAddressUseCase addressUseCase)
        {
            _getAddressUseCase = addressUseCase;
        }

        /// <summary>
        /// Returns an address from the given addressID or LPI_Key
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        [HttpGet, MapToApiVersion("1")]
        [Route("{addressID}")]
        [ProducesResponseType(typeof(APIResponse<SearchAddressResponse>), 200)]
        public IActionResult GetAddress(string addressId)
        {
            var request = new GetAddressRequest { addressID = addressId };
            var response = _getAddressUseCase.ExecuteAsync(request);

            return HandleResponse(response);
        }

        ///<summary>
        ///return options
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            Response.Headers.AccessControlAllowMethods = "GET,OPTIONS";
            Response.Headers.AccessControlAllowOrigin = "*";
            Response.Headers.AccessControlAllowHeaders = "*";
            return HandleResponse("OK");
        }
    }
}
