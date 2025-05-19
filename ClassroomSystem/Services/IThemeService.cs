using System.Threading.Tasks;

namespace ClassroomSystem.Services
{
    public interface IThemeService
    {
        Task<string> GetCurrentThemeAsync(string userId);
        Task SetThemeAsync(string userId, string theme);
        Task ToggleThemeAsync(string userId);
    }
} 