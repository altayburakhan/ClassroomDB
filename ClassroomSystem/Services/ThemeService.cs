using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;
using ClassroomSystem.Models;

namespace ClassroomSystem.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ThemeCookieName = "ThemePreference";
        private const string DefaultTheme = "light";
        private const string LIGHT_THEME = "light";
        private const string DARK_THEME = "dark";

        public ThemeService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetCurrentThemeAsync(string userId)
        {
            // If no userId is provided, return the default theme
            if (string.IsNullOrEmpty(userId))
            {
                return DefaultTheme;
            }

            var preference = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preference == null)
            {
                preference = new UserPreference
                {
                    UserId = userId,
                    Theme = LIGHT_THEME
                };
                _context.UserPreferences.Add(preference);
                await _context.SaveChangesAsync();
            }

            return preference.Theme;
        }

        public async Task ToggleThemeAsync(string userId)
        {
            var preference = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preference == null)
            {
                preference = new UserPreference
                {
                    UserId = userId,
                    Theme = DARK_THEME
                };
                _context.UserPreferences.Add(preference);
            }
            else
            {
                preference.Theme = preference.Theme == LIGHT_THEME ? DARK_THEME : LIGHT_THEME;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetThemeAsync(string userId, string theme)
        {
            if (string.IsNullOrEmpty(theme))
            {
                theme = DefaultTheme;
            }

            // Set cookie
            SetThemeCookie(theme);

            // Update database if user is logged in
            if (!string.IsNullOrEmpty(userId))
            {
                var userPreference = await _context.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (userPreference == null)
                {
                    userPreference = new UserPreference
                    {
                        UserId = userId,
                        Theme = theme
                    };
                    _context.UserPreferences.Add(userPreference);
                }
                else
                {
                    userPreference.Theme = theme;
                }

                await _context.SaveChangesAsync();
            }
        }

        private void SetThemeCookie(string theme)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddYears(1)
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(ThemeCookieName, theme, cookieOptions);
        }
    }
} 