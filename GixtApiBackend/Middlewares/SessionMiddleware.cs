using GixtApiBackend.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GixtApi.Middlewares
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        public async Task Invoke(HttpContext context, AppDbContext db)
        {
            var user = context.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userIdClaim = user.FindFirst("user_id")?.Value;
                var versionClaim = user.FindFirst("token_version")?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) &&
                    !string.IsNullOrEmpty(versionClaim))
                {
                    var userId = Guid.Parse(userIdClaim);
                    var tokenVersion = int.Parse(versionClaim);

                    var session = await db.sessions
                      .FirstOrDefaultAsync(s => s.user_id == userId && s.is_active);

                    if (session == null || session.token_version != tokenVersion)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Session expired");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

}
