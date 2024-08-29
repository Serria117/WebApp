namespace WebApp.Services.CommonService;
using BCrypt.Net;

public static class BcryptService
{
    public static string PasswordEncode(this string raw)
    {
        return BCrypt.EnhancedHashPassword(raw, 12);
    }

    public static bool PasswordVerify(this string raw, string hashed)
    {
        return BCrypt.EnhancedVerify(raw, hashed);
    }
}