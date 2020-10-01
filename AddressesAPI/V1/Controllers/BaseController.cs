using AddressesAPI.V1.Boundary.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AddressesAPI.V1.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
            ConfigureJsonSerializer();
        }

        private static void ConfigureJsonSerializer()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat
                };


                return settings;
            };
        }

        protected IActionResult HandleResponse<T>(T result) where T : class
        {
            var apiResponse = new APIResponse<T>(result);
            //Set a statusCode as well as an object
            return StatusCode(apiResponse.StatusCode, apiResponse);
        }
    }
}
