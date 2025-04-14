using Processor.Models.Enums;

namespace Processor.Models;

public class Incident
{
    public Guid Id { get; set; }
    public IncidentTypeEnum Type { get; set; }
    public DateTime Time { get; set; }
    public List<Event> Events { get; set; } = new List<Event>();
}