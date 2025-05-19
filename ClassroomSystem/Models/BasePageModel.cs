using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ClassroomSystem.Models
{
    public abstract class BasePageModel : PageModel
    {
        protected string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
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