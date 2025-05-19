using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ClassroomSystem.Models
{
    public abstract class BasePageModel : PageModel
    {
        protected readonly ILogger<BasePageModel> _logger;

        protected BasePageModel(ILogger<BasePageModel> logger)
        {
            _logger = logger;
        }

        protected string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("GetCurrentUserId called. User authenticated: {IsAuthenticated}, UserId: {UserId}",
                User.Identity?.IsAuthenticated, userId);
            return userId;
        }

        protected bool IsUserLoggedIn()
        {
            return User.Identity?.IsAuthenticated ?? false;
        }

        protected bool IsUserInRole(string role)
        {
            return User.IsInRole(role);
        }

        protected string GetCurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email);
        }

        protected string GetCurrentUserName()
        {
            return User.FindFirstValue(ClaimTypes.Name);
        }
    }
} 