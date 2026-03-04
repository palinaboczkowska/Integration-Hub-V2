using IntegrationHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationsController(IIntegrationService integrationService, ILogger<IntegrationsController> logger)
    : ControllerBase
{
    private readonly IIntegrationService _integrationService = integrationService;
    private readonly ILogger<IntegrationsController> _logger = logger;

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Fetching available integrations");
        var integrations = _integrationService.GetAvailableIntegrations();
        return Ok(integrations);
    }

    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        _logger.LogInformation("Fetching integration hub settings");
        var settings = _integrationService.GetSettings();
        return Ok(settings);
    }
}

