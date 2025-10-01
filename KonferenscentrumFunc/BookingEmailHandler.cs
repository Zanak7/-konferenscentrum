using System.Text.Json;
using Azure.Communication.Email;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace KonferenscentrumFunc;

public class BookingEmailHandler
{
    private readonly ILogger<BookingEmailHandler> _logger;
    private readonly EmailClient _emailClient;
    private readonly string _from;

    public BookingEmailHandler(ILogger<BookingEmailHandler> logger, IConfiguration config)
    {
        _logger = logger;
        _emailClient = new EmailClient(config["EmailConnectionString"]);
        _from = config["EmailFromAddress"] ?? "donotreply@example.com";
    }

    [Function("BookingEmailHandler")]
    public async Task Run([QueueTrigger("booking-emails", Connection = "AzureWebJobsStorage")] string message)
    {
        // Always log that a message has been received so it appears in Log Stream / Monitor
        _logger.LogInformation("Queue message received at {Time}", DateTime.UtcNow);
        _logger.LogInformation("Raw queue message: {Message}", message);

        // Try to parse JSON. If parsing fails, attempt Base64 decode and parse again.
        string json = message;
        Dictionary<string, object>? data = null;
        try
        {
            data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }
        catch (Exception parseEx)
        {
            _logger.LogWarning(parseEx, "Initial JSON parse failed; attempting Base64 decode");
            try
            {
                var bytes = Convert.FromBase64String(message);
                json = System.Text.Encoding.UTF8.GetString(bytes);
                _logger.LogInformation("Decoded Base64 queue message: {Decoded}", json);
                data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception decodeEx)
            {
                _logger.LogError(decodeEx, "Failed to parse queue message as JSON even after Base64 decode. Message will be ignored.");
            }
        }

        if (data == null)
        {
            // Nothing we can do with this message, but we logged the problem so it shows in Azure logs
            _logger.LogInformation("Queue message could not be parsed to JSON and will be skipped.");
            return;
        }

        var to = data.ContainsKey("CustomerEmail") ? data["CustomerEmail"]?.ToString() : "test@example.com";
        var action = data.ContainsKey("Action") ? data["Action"]?.ToString() : "Okänd";
        var name = data.ContainsKey("CustomerName") ? data["CustomerName"]?.ToString() : "Kund";

        // Create email body
        var subject = $"Bekräftelse på bokning: {action}";
        var text = $"Hej {name},\nDin bokning är {action}.";

        try
        {
            var email = new EmailMessage(_from, to, new EmailContent(subject) { PlainText = text });
            var result = await _emailClient.SendAsync(WaitUntil.Completed, email);
            _logger.LogInformation("Email sent, status: {Status}", result.Value.Status);
        }
        catch (Exception ex)
        {
            // Log full exception so it appears in Application Insights / Log Stream
            _logger.LogError(ex, "Error sending email");
        }
    }
}
