using System.Security.Claims;
using Bankmore.Accounts.Command.Api.Contracts;
using Bankmore.Accounts.Command.Application.Commands.Accounts.ActivateAccount;
using Bankmore.Accounts.Command.Application.Commands.Accounts.CreateAccount;
using Bankmore.Accounts.Command.Application.Commands.Accounts.DeactivateAccount;
using Bankmore.Shared.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bankmore.Accounts.Command.Api.Controllers;

[ApiController]
[Route("v1/Contas")]
public sealed class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AccountsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [HttpPost("Criar")]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand cmd, CancellationToken ct)
    {
        var res = await _mediator.Send(cmd, ct);
        return Ok(res);
    }

    [Authorize] 
    [HttpPost("desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate([FromBody] DeactivateAccountRequest req, CancellationToken ct)
    {
        var token = TokenUtils.ExtractToken(Request.Headers["Authorization"]);
        if (string.IsNullOrWhiteSpace(token))
            return Forbid();
        
        var numeroConta = req.NumeroConta.GetValueOrDefault();

        if (numeroConta <= 0)
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(claim, out numeroConta) || numeroConta <= 0)
                return Forbid();
        }

        await _mediator.Send(new DeactivateAccountCommand(numeroConta, req.Password, token), ct);
        return NoContent(); 
    }

    [Authorize]
    [HttpPost("ativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Activate([FromBody] ActivateAccountRequest req, CancellationToken ct)
    {
        var token = TokenUtils.ExtractToken(Request.Headers["Authorization"]);
        if (string.IsNullOrWhiteSpace(token))
            return Forbid();
        
        var numeroConta = req.NumeroConta.GetValueOrDefault();

        if (numeroConta <= 0)
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(claim, out numeroConta) || numeroConta <= 0)
                return Forbid();
        }

        await _mediator.Send(new ActivateAccountCommand(numeroConta, req.Password, token), ct);
        return NoContent();
    }
}