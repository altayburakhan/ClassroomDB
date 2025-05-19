using System;
using Microsoft.EntityFrameworkCore;
using ClassroomSystem.Data;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClassroomSystem.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected readonly ApplicationDbContext Context;
        protected readonly Mock<ILogger<TestBase>> LoggerMock;

        protected TestBase()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            Context = new ApplicationDbContext(options);

            // Setup logger mock
            LoggerMock = new Mock<ILogger<TestBase>>();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }

        protected void SeedTestData()
        {
            // Add test data here
            Context.SaveChanges();
        }
    }
} 