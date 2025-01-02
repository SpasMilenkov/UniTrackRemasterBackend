using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public static class CookieOptionManager
{

    private static string _cookieDomain;

    public static void Initialize(IConfiguration configuration)
    {
        _cookieDomain = configuration["CookieDomain"] ?? "localhost";
    }

    public static CookieOptions GenerateRefreshCookieOptions()
    {
        return new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.UtcNow.AddHours(2),
            Domain = _cookieDomain,
            IsEssential = true
        };
    }
    public static CookieOptions GenerateAccessCookieOptions()
    {
        return new CookieOptions()
        { 
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.UtcNow.AddMinutes(2),
            Domain = _cookieDomain,
            IsEssential = true
        };
    }
} 