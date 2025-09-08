using Bankmore.Accounts.Query.Api.Contracts;
using Bankmore.Accounts.Query.Application.Queries.Accounts.GetBalance;
using Bankmore.Shared.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bankmore.Accounts.Query.Api.Controllers;

[ApiController]
[Route("v1/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator) => _mediator = mediator;

     [Authorize]
     [HttpGet("contas/{contaId}/saldo")]
     public async Task<IActionResult> ObterSaldo([FromRoute] int contaId, CancellationToken ct)
     {
         var token = TokenUtils.ExtractToken(Request.Headers["Authorization"]);
     
         var result = await _mediator.Send(new GetBalanceQuery(contaId, token), ct);
     
         if (result is null)
             return NotFound();
     
         return Ok(new BalanceResponse()
         {
             SaldoDisponivel = result.SaldoDisponivel,
             Nome = result.Nome,
             NumeroDaConta = result.NumerConta,
             AsOfUtc = result.AsOfUtc
         });
     }
}