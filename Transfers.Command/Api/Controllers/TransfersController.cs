using Api.Contracts;
using Application.Commands.Transfers;
using Bankmore.Shared.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bankmore.Transfers.Command.Api.Controllers;

[ApiController]
[Route("v1/Transacoes")]
public sealed class TransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransfersController(IMediator mediator)
    {
        _mediator = mediator;   
    }

    [Authorize]
    [HttpPost("transferencias")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferBetweenAccountsRequest body,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new { error = "Missing Idempotency-Key header." });
        var idempKey = Request.Headers["Idempotency-Key"].ToString();
        
        var token = TokenUtils.ExtractFullToken(Request.Headers["Authorization"]);

        var cmd = new TransferBetweenAccountsCommand(
            body.OrigemAccountId,
            body.DestinoAccountId,
            body.Valor,
            idempKey,
            token
        );
    
        var result = await _mediator.Send(cmd, ct);
    
        if (result.Success)
            return NoContent();
    
        throw new Exception("Falha na transferência.");
    }
}