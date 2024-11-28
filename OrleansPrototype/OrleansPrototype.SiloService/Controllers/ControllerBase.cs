using Microsoft.AspNetCore.Mvc;

namespace OrleansPrototype.SiloService.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
}
