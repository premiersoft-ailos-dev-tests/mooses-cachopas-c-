namespace Bankmore.Accounts.Command.Application.Abstractions;

public interface IPasswordHasher
{
    (string Hash, string Salt) HashPassword(string plain);
    bool Verify(string plain, string hash, string salt);
}