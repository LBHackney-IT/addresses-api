using System.Collections.Generic;
using AddressesAPI.V2.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V2.Controllers
{
    [ApiVersion("2")]
    [Produces("application/json")]
    [Route("api/v1/healthcheck")]
    [ApiController]
    public class HealthCheckController : V1.Controllers.BaseController
    {
        [HttpGet]
        [Route("ping")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), 200)]
        public IActionResult HealthCheck()
        {
            var result = new Dictionary<string, bool> { { "success", true } };

            return Ok(result);
        }

        [HttpGet]
        [Route("error")]
        public void ThrowError()
        {
            ThrowOpsErrorUsecase.Execute();
        }
    }
}
