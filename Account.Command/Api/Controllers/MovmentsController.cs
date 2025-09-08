using System.Security.Claims;
using Bankmore.Accounts.Command.Api.Contracts.Transactions;
using Bankmore.Accounts.Command.Application.Commands.Transactions.Movments;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Bankmore.Accounts.Command.Domain.Extensions.Movments;
using Bankmore.Shared.Utils;

namespace Bankmore.Accounts.Command.Api.Controllers;

[ApiController]
[Route("v1/contas/movimento")]
public class MovmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MovmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Movment(
        [FromBody] MovmentRequest body,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new { error = "Missing Idempotency-Key header." });

        var token = TokenUtils.ExtractToken(Request.Headers["Authorization"]);
        if (string.IsNullOrWhiteSpace(token))
            return Forbid();

        var cmd = new MovmentCommand(body.NumeroConta, body.Valor, token, body.TipoOperacao.ToMovmentType(), idempotencyKey);

        var result = await _mediator.Send(cmd, ct);

        if (result.Success)
            return NoContent();

        throw new BusinessRuleException(result.ErrorMessage ?? "Falha no depósito.", result.ErrorType ?? "BUSINESS_ERROR");
    }
}