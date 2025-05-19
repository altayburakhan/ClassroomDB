using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ClassroomSystem.Services;
using ClassroomSystem.Data;
using ClassroomSystem.Models;

namespace ClassroomSystem.Tests
{
    public class ThemeServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IResponseCookies> _mockResponseCookies;
        private readonly Mock<IRequestCookieCollection> _mockRequestCookies;
        private readonly ApplicationDbContext _context;
        private readonly ThemeService _themeService;

        public ThemeServiceTests()
        {
            // Setup mocks
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockResponseCookies = new Mock<IResponseCookies>();
            _mockRequestCookies = new Mock<IRequestCookieCollection>();

            _mockHttpContext.Setup(x => x.Response.Cookies).Returns(_mockResponseCookies.Object);
            _mockHttpContext.Setup(x => x.Request.Cookies).Returns(_mockRequestCookies.Object);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _themeService = new ThemeService(_context, _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task GetCurrentThemeAsync_NoCookieNoUser_ReturnsDefaultTheme()
        {
            // Arrange
            _mockRequestCookies.Setup(x => x[It.IsAny<string>()]).Returns((string)null);

            // Act
            var result = await _themeService.GetCurrentThemeAsync(null);

            // Assert
            Assert.Equal("light", result);
        }

        [Fact]
        public async Task GetCurrentThemeAsync_WithCookie_ReturnsCookieTheme()
        {
            // Arrange
            _mockRequestCookies.Setup(x => x["ThemePreference"]).Returns("dark");

            // Act
            var result = await _themeService.GetCurrentThemeAsync("user1");

            // Assert
            Assert.Equal("dark", result);
        }

        [Fact]
        public async Task GetCurrentThemeAsync_WithUserPreference_ReturnsUserTheme()
        {
            // Arrange
            _mockRequestCookies.Setup(x => x[It.IsAny<string>()]).Returns((string)null);
            var userPreference = new UserPreference
            {
                UserId = "user1",
                Theme = "dark"
            };
            _context.UserPreferences.Add(userPreference);
            await _context.SaveChangesAsync();

            // Act
            var result = await _themeService.GetCurrentThemeAsync("user1");

            // Assert
            Assert.Equal("dark", result);
        }

        [Fact]
        public async Task SetThemeAsync_WithUserId_UpdatesDatabase()
        {
            // Arrange
            var userId = "user1";

            // Act
            await _themeService.SetThemeAsync(userId, "dark");

            // Assert
            var preference = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            Assert.NotNull(preference);
            Assert.Equal("dark", preference.Theme);
        }

        [Fact]
        public async Task ToggleThemeAsync_FromLightToDark_ChangesTheme()
        {
            // Arrange
            var userId = "user1";
            _mockRequestCookies.Setup(x => x["ThemePreference"]).Returns("light");

            // Act
            await _themeService.ToggleThemeAsync(userId);

            // Assert
            var preference = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            Assert.NotNull(preference);
            Assert.Equal("dark", preference.Theme);
        }

        [Fact]
        public async Task ToggleThemeAsync_FromDarkToLight_ChangesTheme()
        {
            // Arrange
            var userId = "user1";
            _mockRequestCookies.Setup(x => x["ThemePreference"]).Returns("dark");

            // Act
            await _themeService.ToggleThemeAsync(userId);

            // Assert
            var preference = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            Assert.NotNull(preference);
            Assert.Equal("light", preference.Theme);
        }
    }
} 