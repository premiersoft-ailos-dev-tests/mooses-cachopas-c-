using System.Text.RegularExpressions;
using Bankmore.Accounts.Command.Api.Contracts;
using Bankmore.Accounts.Command.Api.Security;
using Bankmore.Accounts.Command.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Bankmore.Accounts.Command.Api.Controllers;

[ApiController]
[Route("v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthReadStore _authReadStore;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthController(IAuthReadStore authReadStore, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _authReadStore = authReadStore;
        _hasher = hasher;
        _jwt = jwt;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if ((req.Numero is null || req.Numero <= 0) && string.IsNullOrWhiteSpace(req.Cpf))
            return UnauthorizedJson("Informe o número da conta ou o CPF.");
        
        (bool found, string accountId, string senhaHash, string salt) creds;

        if (req.Numero is not null && req.Numero > 0)
        {
            creds = await _authReadStore.GetCredentialsByNumeroAsync(req.Numero.Value, ct);
        }
        else
        {
            var onlyDigits = Regex.Replace(req.Cpf!, "[^0-9]", "");
            if (onlyDigits.Length != 11)
                return UnauthorizedJson("CPF inválido.");
            creds = await _authReadStore.GetCredentialsByCpfAsync(onlyDigits, ct);
        }

        if (!creds.found)
            return UnauthorizedJson("Usuário não encontrado ou inativo.");

        if (!_hasher.Verify(req.Senha, creds.senhaHash, creds.salt))
            return UnauthorizedJson("Senha inválida.");

        var (token, exp) = _jwt.CreateForAccount(creds.accountId);

        return Ok(new LoginResponse
        {
            AccessToken = token,
            ExpiresAtUtc = exp,
            AccountId = creds.accountId
        });
    }

    private IActionResult UnauthorizedJson(string message)
        => StatusCode(StatusCodes.Status401Unauthorized, new { type = "USER_UNAUTHORIZED", message });
}