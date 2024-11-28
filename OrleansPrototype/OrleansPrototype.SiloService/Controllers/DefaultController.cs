using Microsoft.AspNetCore.Mvc;

namespace OrleansPrototype.SiloService.Controllers;

public class DefaultController : ControllerBase
{
    private readonly ILogger<DefaultController> _logger;

    public DefaultController(ILogger<DefaultController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Test()
    {
        return Ok();
    }
}
