using Generator.Models;

namespace Generator.Services
{
    public class EventGeneratorBackgroundService : BackgroundService
    {
        private readonly ILogger<EventGeneratorBackgroundService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        
        private readonly string _processorEndpoint = "http://processor/api/events";

        public EventGeneratorBackgroundService(ILogger<EventGeneratorBackgroundService> logger,
                                               IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис генератора событий запущен.");

            var rnd = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                int delayMs = rnd.Next(0, 2000);
                await Task.Delay(delayMs, stoppingToken);
                
                var generatedEvent = new Event
                {
                    Id = Guid.NewGuid(),
                    Type = (EventTypeEnum)rnd.Next(1, 4),
                    Time = DateTime.UtcNow
                };

                _logger.LogInformation("Сгенерировано событие: {@Event}", generatedEvent);
                
                try
                {
                    var client = _clientFactory.CreateClient();
                    var response = await client.PostAsJsonAsync(_processorEndpoint, generatedEvent, stoppingToken);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при отправке события.");
                }
            }
        }
    }
}