using IntegrationHub.Api.Models;
using IntegrationHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationHub.Api.Controllers;

[ApiController]
public class FreshdeskWebhookController(ITicketSyncService ticketSyncService, ILogger<FreshdeskWebhookController> logger)
    : ControllerBase
{
    private readonly ITicketSyncService _ticketSyncService = ticketSyncService;
    private readonly ILogger<FreshdeskWebhookController> _logger = logger;

    public class FreshdeskWebhookPayload
    {
        public FreshdeskTicket Ticket { get; set; } = new();
    }

    [HttpPost("/webhooks/freshdesk/ticket")]
    public async Task<IActionResult> HandleTicketAsync([FromBody] FreshdeskWebhookPayload payload, CancellationToken cancellationToken)
    {
        if (payload?.Ticket == null)
        {
            return BadRequest("Missing ticket data in webhook payload.");
        }

        var ticket = payload.Ticket;

        _logger.LogInformation("Received Freshdesk ticket webhook for ticket {TicketId}", ticket.Id);

        var asanaTaskId = await _ticketSyncService.SyncFromFreshdeskTicketAsync(ticket, cancellationToken);

        return Ok(new { asanaTaskId });
    }
}

