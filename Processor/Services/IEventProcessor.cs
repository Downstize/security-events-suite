using Processor.Models;

namespace Processor.Services;

public interface IEventProcessor
{
    Task ProcessEventAsync(Event evt);
}