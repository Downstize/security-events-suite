using Microsoft.AspNetCore.Mvc;
using Generator.Models;

namespace Generator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        
        private readonly string _processorEndpoint = "http://processor/api/events";

        public EventController(ILogger<EventController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }
        
        [HttpPost("manual")]
        public async Task<IActionResult> GenerateEventManually([FromQuery] int type)
        {
            var evt = new Event
            {
                Id = Guid.NewGuid(),
                Type = (EventTypeEnum)type,
                Time = DateTime.UtcNow
            };

            _logger.LogInformation("Ручное событие: {@Event}", evt);

            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsJsonAsync(_processorEndpoint, evt);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки события вручную.");
                return StatusCode(500, "Ошибка при отправке события.");
            }

            return Ok(evt);
        }
    }
}