using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KonferenscentrumVast.Services
{
    public class BookingEmailQueueService
    {
        private readonly QueueClient _queue;
        private readonly ILogger<BookingEmailQueueService> _logger;

        public BookingEmailQueueService(IConfiguration config, ILogger<BookingEmailQueueService> logger)
        {
            _logger = logger;

            var conn = config["StorageConnectionString"];
            var name = "booking-emails";          //  booking-emails

            var options = new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            };
            
            _queue = new QueueClient(conn, name, options);
            _queue.CreateIfNotExists();
        }

        public async Task EnqueueAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            await _queue.SendMessageAsync(json);
            _logger.LogInformation("Lade meddelande på kö.");
        }
    }
}
