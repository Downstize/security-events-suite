using System.Collections.Concurrent;
using Processor.Data;
using Processor.Models;
using Processor.Models.Enums;

namespace Processor.Services
{
    public class EventProcessor : IEventProcessor
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventProcessor> _logger;
        private readonly ConcurrentDictionary<Guid, (Event evt, CancellationTokenSource cts)> _pendingType2Events = new();
        private readonly ConcurrentDictionary<Guid, (Event evt, CancellationTokenSource cts)> _pendingType3Events = new();

        public EventProcessor(ApplicationDbContext context, ILogger<EventProcessor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessEventAsync(Event evt)
        {
            _logger.LogInformation("Обработка полученного события: {@Event}", evt);
            switch (evt.Type)
            {
                case EventTypeEnum.Type1:
                    await CreateIncidentAsync(new Incident
                    {
                        Id = Guid.NewGuid(),
                        Type = IncidentTypeEnum.Type1,
                        Time = DateTime.UtcNow,
                        Events = new List<Event> { evt }
                    });
                    CompletePendingType2WithType1(evt);
                    break;
                case EventTypeEnum.Type2:
                    var cts20 = new CancellationTokenSource();
                    _pendingType2Events.TryAdd(evt.Id, (evt, cts20));
                    _ = HandleType2DelayedProcessing(evt, cts20.Token);
                    break;
                case EventTypeEnum.Type3:
                    var cts60 = new CancellationTokenSource();
                    _pendingType3Events.TryAdd(evt.Id, (evt, cts60));
                    _ = HandleType3DelayedProcessing(evt, cts60.Token);
                    break;
            }
        }

        private async Task HandleType2DelayedProcessing(Event evt, CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(20), token);
                if (_pendingType2Events.TryRemove(evt.Id, out _))
                {
                    _logger.LogInformation("Таймаут шаблона №2 - создаём инцидент Type1 для события: {@Event}", evt);
                    await CreateIncidentAsync(new Incident
                    {
                        Id = Guid.NewGuid(),
                        Type = IncidentTypeEnum.Type1,
                        Time = DateTime.UtcNow,
                        Events = new List<Event> { evt }
                    });
                }
            }
            catch (TaskCanceledException) { }
        }

        private async Task HandleType3DelayedProcessing(Event evt, CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(60), token);
                if (_pendingType3Events.TryRemove(evt.Id, out _))
                {
                    _logger.LogInformation("Таймаут шаблона №3 - создаём инцидент Type1 для события: {@Event}", evt);
                    await CreateIncidentAsync(new Incident
                    {
                        Id = Guid.NewGuid(),
                        Type = IncidentTypeEnum.Type1,
                        Time = DateTime.UtcNow,
                        Events = new List<Event> { evt }
                    });
                }
            }
            catch (TaskCanceledException) { }
        }

        private bool CompletePendingType2WithType1(Event evtType1)
        {
            bool processed = false;
            foreach (var pending in _pendingType2Events.ToArray())
            {
                var (pendingEvent, cts) = pending.Value;
                if ((evtType1.Time - pendingEvent.Time).TotalSeconds <= 20)
                {
                    cts.Cancel();
                    if (_pendingType2Events.TryRemove(pending.Key, out _))
                    {
                        _logger.LogInformation("Получено событие Type1 для завершения шаблона №2 - создаём инцидент Type2.");
                        Incident incidentType2 = new Incident
                        {
                            Id = Guid.NewGuid(),
                            Type = IncidentTypeEnum.Type2,
                            Time = DateTime.UtcNow,
                            Events = new List<Event> { pendingEvent, evtType1 }
                        };
                        _ = CreateIncidentAsync(incidentType2);
                        CompletePendingType3WithIncidentType2(incidentType2);
                        processed = true;
                    }
                }
            }
            return processed;
        }

        private void CompletePendingType3WithIncidentType2(Incident incidentType2)
        {
            foreach (var pending in _pendingType3Events.ToArray())
            {
                var (pendingEvent, cts) = pending.Value;
                if ((incidentType2.Time - pendingEvent.Time).TotalSeconds <= 60)
                {
                    cts.Cancel();
                    if (_pendingType3Events.TryRemove(pending.Key, out _))
                    {
                        _logger.LogInformation("Поступил инцидент Type2 после события Type3 - создаём инцидент Type3.");
                        _ = CreateIncidentAsync(new Incident
                        {
                            Id = Guid.NewGuid(),
                            Type = IncidentTypeEnum.Type3,
                            Time = DateTime.UtcNow,
                            Events = new List<Event> { pendingEvent }
                        });
                    }
                }
            }
        }

        private async Task CreateIncidentAsync(Incident incident)
        {
            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Инцидент создан: {@Incident}", incident);
        }
    }
}
