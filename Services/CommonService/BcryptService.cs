namespace WebApp.Services.CommonService;
using BCrypt.Net;

public static class BcryptService
{
    private const int Factor = 10;
    public static string BCryptHash(this string raw)
    {
        return BCrypt.HashPassword(raw, Factor);
    }

    public static bool PasswordVerify(this string raw, string hashed)
    {
        return BCrypt.Verify(raw, hashed);
    }

    public static string ChangePassword(this string newPwd, string oldPwd, string hashed)
    {
        return BCrypt.ValidateAndReplacePassword(
            currentKey: oldPwd,
            currentHash: hashed,
            newKey: newPwd,
            workFactor: Factor
        );
    }
}