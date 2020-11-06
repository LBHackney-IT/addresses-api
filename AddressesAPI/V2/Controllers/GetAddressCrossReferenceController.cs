using System;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V2.Controllers
{
    [ApiVersion("2")]
    [Produces("application/json")]
    [Route("api/v2/properties")]
    [ProducesResponseType(typeof(APIResponse<object>), 400)]
    [ProducesResponseType(typeof(APIResponse<object>), 500)]
    public class GetAddressCrossReferenceController : BaseController
    {
        private readonly IGetAddressCrossReferenceUseCase _getAddressCrossReferenceUseCase;
        public GetAddressCrossReferenceController(IGetAddressCrossReferenceUseCase getAddressCrossReferenceUseCase)
        {
            _getAddressCrossReferenceUseCase = getAddressCrossReferenceUseCase;
        }

        /// <summary>
        /// Search Controller V1 to search for addresses
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        /// <param name="uprn"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(APIResponse<GetAddressCrossReferenceResponse>), 200)]
        [HttpGet]
        [Route("{uprn}/crossreferences")]
        public IActionResult GetAddressCrossReference(long uprn)
        {
            var request = new GetAddressCrossReferenceRequest { uprn = uprn };
            try
            {
                var response = _getAddressCrossReferenceUseCase.ExecuteAsync(request);
                if (response == null)
                {
                    return new NotFoundResult();
                }
                return new OkObjectResult(new APIResponse<GetAddressCrossReferenceResponse>(response));
            }
            catch (BadRequestException e)
            {
                return new BadRequestObjectResult(e.ValidationResponse);
            }

        }
    }
}
