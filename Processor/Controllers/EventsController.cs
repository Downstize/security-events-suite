using Microsoft.AspNetCore.Mvc;
using Processor.Models;
using Processor.Services;

namespace Processor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventProcessor _eventProcessor;

    public EventsController(IEventProcessor eventProcessor)
    {
        _eventProcessor = eventProcessor;
    }
    
    [HttpPost]
    public async Task<IActionResult> ReceiveEvent([FromBody] Event evt)
    {
        await _eventProcessor.ProcessEventAsync(evt);
        return Ok();
    }
}