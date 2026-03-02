using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.UnitOfWork.Extensions;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

namespace EntityFrameworkCore.Tests
{
    [CollectionDefinition("PostgreSql")]
    public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
    { }

    /// <summary>
    /// Shared fixture for PostgreSQL integration tests. Uses a single container across all tests
    /// to avoid slow startup and flakiness under parallel execution.
    /// </summary>
    public class PostgreSqlFixture : IAsyncLifetime
    {
        private PostgreSqlContainer _postgresContainer;
        private bool _initialized;

        public PostgreSqlFixture()
        { }

        /// <summary>
        /// If set, Docker/Testcontainers was unavailable and all tests should skip.
        /// </summary>
        public string SkipReason { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                _postgresContainer = new PostgreSqlBuilder()
                    .WithImage("postgres:17-alpine")
                    .WithDatabase("blogging_test")
                    .WithUsername("testuser")
                    .WithPassword("testpass")
                    .Build();

                await _postgresContainer.StartAsync();
            }
            catch (Exception ex)
            {
                SkipReason = $"Docker/Testcontainers unavailable: {ex.Message}";
                return;
            }

            var services = new ServiceCollection();

            services.AddDbContext<BloggingContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()),
                ServiceLifetime.Scoped);

            services.AddScoped<DbContext, BloggingContext>();
            services.AddUnitOfWork();
            services.AddUnitOfWork<BloggingContext>();

            ServiceProvider = services.BuildServiceProvider();

            using var scope = ServiceProvider.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var context = scope.ServiceProvider.GetRequiredService<BloggingContext>();

            await context.Database.EnsureCreatedAsync();
            await SeedDataAsync(unitOfWork);

            _initialized = true;
        }

        private static async Task SeedDataAsync(IUnitOfWork unitOfWork)
        {
            var repository = unitOfWork.Repository<Blog>();

            var blogs = Seeder.SeedBlogs();

            await repository.AddRangeAsync(blogs);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            if (_postgresContainer != null)
            {
                await _postgresContainer.DisposeAsync();
            }
        }
    }
}
