using Bankmore.Accounts.Command.Application.Abstractions;
using BCrypt.Net;

namespace Bankmore.Accounts.Command.Infrastructure.Services;
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public (string Hash, string Salt) HashPassword(string plain)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        var hash = BCrypt.Net.BCrypt.HashPassword(plain, salt);
        return (hash, salt);
    }

    public bool Verify(string plain, string hash, string salt)
    {
        return BCrypt.Net.BCrypt.Verify(plain, hash);
    }
}