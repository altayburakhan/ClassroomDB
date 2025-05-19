using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClassroomSystem.Services;
using System.Threading.Tasks;

namespace ClassroomSystem.Controllers
{
    [Route("api/theme")]
    [Authorize]
    public class ThemeController : Controller
    {
        private readonly IThemeService _themeService;

        public ThemeController(IThemeService themeService)
        {
            _themeService = themeService;
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleTheme()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _themeService.ToggleThemeAsync(userId);
            var newTheme = await _themeService.GetCurrentThemeAsync(userId);
            return Content(newTheme);
        }
    }
} 