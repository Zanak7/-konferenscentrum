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
    public async Task Run([QueueTrigger("booking-emails", Connection = "AzureWebJobsStorage")] 
        BookingMessageModel message)
    {
        // Always log that a message has been received so it appears in Log Stream / Monitor
        _logger.LogInformation("[SHO] Queue message received at {Time}", DateTime.UtcNow);
        _logger.LogInformation("[SHO] BookingId {BookingId}, Action {Action}", message.BookingId, message.Action);

        var to = message.CustomerEmail;
        var action = message.Action;
        var name = message.CustomerName;

        // Create email body
        var subject = $"Status på bokningsnummer {message.BookingId}: {action}";
        var text = $"Hej {name},\n\nDin bokning är {action}.";
        if (message.Reason is not null)
        {
            text += $"\nReason: {message.Reason}.";
        }

        try
        {
            var email = new EmailMessage(_from, to, new EmailContent(subject) { PlainText = text });
            var result = await _emailClient.SendAsync(WaitUntil.Completed, email);
            _logger.LogInformation("[SHO] Email sent for BookingId {BookingId}, status: {Status}",
                message.BookingId, result.Value.Status);
        }
        catch (Exception ex)
        {
            // Log full exception so it appears in Application Insights / Log Stream
            _logger.LogError(ex, "[SHO] Error sending email, BookingId {BookingId}", message.BookingId);
        }
    }
}


