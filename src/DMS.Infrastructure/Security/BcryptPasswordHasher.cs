using DMS.Application.Interfaces;

namespace DMS.Infrastructure.Security;

internal sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        var normalizedHash = hash.Trim();

        try
        {
            if (BCrypt.Net.BCrypt.Verify(password, normalizedHash))
            {
                return true;
            }

            // Backward-compatible path for hashes generated with EnhancedHashPassword.
            return BCrypt.Net.BCrypt.EnhancedVerify(password, normalizedHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
