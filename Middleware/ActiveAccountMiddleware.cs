using System.Security.Claims;
using Recruitment_Project.Interfaces.Repositories;

namespace Recruitment_Project.Middleware
{
    public class ActiveAccountMiddleware
    {
        private readonly RequestDelegate _next;

        public ActiveAccountMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IUserRepository userRepository)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null &&
                    int.TryParse(userIdClaim.Value, out int userId))
                {
                    var user = await userRepository.GetByIdAsync(userId);

                    if (user == null || !user.IsActive)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Your account has been disabled."
                        });

                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}