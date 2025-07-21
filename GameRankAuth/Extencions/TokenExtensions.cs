namespace GameRankAuth.Services;

public static class TokenExtensions
{
    public static void SetCookie( this HttpResponse response, string token)
    {
        response.Cookies.Append("myToken", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = false,
            Expires = DateTime.Now.AddDays(1)
        });
    }
}