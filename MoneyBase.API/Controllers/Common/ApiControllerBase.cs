using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MoneyBase.API.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected readonly ISender _mediator;

    protected ApiControllerBase(ISender mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }
}
