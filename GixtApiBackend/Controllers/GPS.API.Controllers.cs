

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GPS.API.Controllers
{
    [ApiController]
    [Route("api/gps")]
    public class GpsController : ControllerBase
    {
        private readonly IHubContext<GpsHub> _hub;

        public GpsController(IHubContext<GpsHub> hub)
        {
            _hub = hub;
        }

    }

    // DTO simple
   
}
