using BrokerService;
using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BridgeController : ControllerBase
    {
        private readonly MqttClientService _bridgeService;

        public BridgeController(MqttClientService bridgeService)
        {
            _bridgeService = bridgeService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartBridge(SubscriptionRequest request)
        {
            await _bridgeService.StartBridgeAsync(request);
            return Ok("Bridge started");
        }

        [HttpPost("stop")]
        public IActionResult StopBridge()
        {
            _bridgeService.StopBridge();
            return Ok("Bridge stopped");
        }
    }
}
