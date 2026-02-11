using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.UnitOfWork.Extensions;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

// ========================================================================================================
// ⚠️  DOCKER REQUIRED TO RUN THESE TESTS  ⚠️
// ========================================================================================================
// These integration tests require Docker Desktop to be installed and running on your local machine.
// 
// Prerequisites:
//   1. Install Docker Desktop: https://www.docker.com/products/docker-desktop
//   2. Start Docker Desktop
//   3. Verify Docker is running: docker ps
//
// Why Docker is needed:
//   - These tests use Testcontainers to spin up a real PostgreSQL database
//   - EF Core's In-Memory provider doesn't support ExecuteUpdate/ExecuteUpdateAsync
//   - Real database ensures bulk update operations work correctly
//
// To run these tests:
//   dotnet test --filter "FullyQualifiedName~RepositoryIntegrationTests"
//
// To skip these tests (run only in-memory tests):
//   dotnet test --filter "FullyQualifiedName!~RepositoryIntegrationTests"
// ========================================================================================================

namespace EntityFrameworkCore.Tests
{
    /// <summary>
    /// Integration tests for Repository using PostgreSQL via Testcontainers.
    /// These tests verify ExecuteUpdate/ExecuteUpdateAsync functionality which doesn't work with InMemory provider.
    /// </summary>
    /// <remarks>
    /// <para><strong>⚠️ DOCKER REQUIRED ⚠️</strong></para>
    /// <para>
    /// These tests require Docker to be running on your local machine. 
    /// Testcontainers will automatically start a PostgreSQL container for testing.
    /// </para>
    /// <para><strong>Prerequisites:</strong></para>
    /// <list type="bullet">
    ///   <item>Docker Desktop must be installed and running</item>
    ///   <item>Docker daemon must be accessible (check with: <c>docker ps</c>)</item>
    ///   <item>Sufficient disk space for PostgreSQL image (~80MB)</item>
    ///   <item>Network access to pull docker.io/postgres:17-alpine image</item>
    /// </list>
    /// <para><strong>To run these tests:</strong></para>
    /// <code>
    /// # Start Docker Desktop first, then:
    /// dotnet test --filter FullyQualifiedName~RepositoryIntegrationTests
    /// </code>
    /// <para>
    /// If Docker is not available, these tests will be skipped automatically with a clear error message.
    /// The standard in-memory tests in <see cref="AsyncDataAccessTests"/> and <see cref="SyncDataAccessTests"/> 
    /// will still run without Docker.
    /// </para>
    /// </remarks>
    public class RepositoryIntegrationTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;
        private IServiceProvider _serviceProvider;
        private IUnitOfWork _unitOfWork;

        public RepositoryIntegrationTests()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17-alpine")
                .WithDatabase("blogging_test")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .Build();
        }

        public async Task InitializeAsync()
        {
            // Start the PostgreSQL container
            await _postgresContainer.StartAsync();

            // Setup services
            var services = new ServiceCollection();

            services.AddDbContext<BloggingContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()),
                ServiceLifetime.Scoped);

            services.AddScoped<DbContext, BloggingContext>();
            services.AddUnitOfWork();
            services.AddUnitOfWork<BloggingContext>();

            _serviceProvider = services.BuildServiceProvider();
            _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            // Create database schema and seed data
            using var scope = _serviceProvider.CreateScope();
            var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var context = scope.ServiceProvider.GetRequiredService<BloggingContext>();
            await context.Database.EnsureCreatedAsync();
            await SeedDataAsync(scopedUnitOfWork);
        }

        public async Task DisposeAsync()
        {
            // Cleanup
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            await _postgresContainer.DisposeAsync();
        }

        private async Task SeedDataAsync(IUnitOfWork unitOfWork)
        {
            var repository = unitOfWork.Repository<Blog>();
            var blogs = Seeder.SeedBlogs();
            await repository.AddRangeAsync(blogs);
            await unitOfWork.SaveChangesAsync();
        }

        #region Add/Insert Tests

        [Fact]
        public async Task AddAsync_ShouldAddNewBlog()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            
            var newBlog = new Blog
            {
                Title = "New Integration Blog",
                Url = "/new-integration-blog",
                TypeId = 1
            };

            // Act
            var addedBlog = await repository.AddAsync(newBlog);
            await unitOfWork.SaveChangesAsync();

            // Assert
            Assert.NotNull(addedBlog);
            Assert.Equal("New Integration Blog", addedBlog.Title);
            Assert.True(addedBlog.Id > 0);

            var count = await repository.CountAsync();
            Assert.Equal(51, count); // 50 seeded + 1 new
        }

        [Fact]
        public async Task AddRangeAsync_ShouldAddMultipleBlogs()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            
            var newBlogs = new List<Blog>
            {
                new Blog { Title = "Integration Blog 1", Url = "/integration-blog-1", TypeId = 1 },
                new Blog { Title = "Integration Blog 2", Url = "/integration-blog-2", TypeId = 2 },
                new Blog { Title = "Integration Blog 3", Url = "/integration-blog-3", TypeId = 1 }
            };

            // Act
            await repository.AddRangeAsync(newBlogs);
            await unitOfWork.SaveChangesAsync();

            // Assert
            var count = await repository.CountAsync();
            Assert.Equal(53, count); // 50 seeded + 3 new
        }

        [Fact]
        public void Add_ShouldAddNewBlog_Sync()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            
            var newBlog = new Blog
            {
                Title = "New Sync Integration Blog",
                Url = "/new-sync-integration-blog",
                TypeId = 1
            };

            // Act
            var addedBlog = repository.Add(newBlog);
            unitOfWork.SaveChanges();

            // Assert
            Assert.NotNull(addedBlog);
            Assert.Equal("New Sync Integration Blog", addedBlog.Title);
            Assert.True(addedBlog.Id > 0);

            var count = repository.Count();
            Assert.Equal(51, count); // 50 seeded + 1 new
        }

        [Fact]
        public void AddRange_ShouldAddMultipleBlogs_Sync()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            
            var newBlogs = new List<Blog>
            {
                new Blog { Title = "Sync Integration Blog 1", Url = "/sync-int-blog-1", TypeId = 1 },
                new Blog { Title = "Sync Integration Blog 2", Url = "/sync-int-blog-2", TypeId = 2 },
                new Blog { Title = "Sync Integration Blog 3", Url = "/sync-int-blog-3", TypeId = 1 }
            };

            // Act
            repository.AddRange(newBlogs);
            unitOfWork.SaveChanges();

            // Assert
            var count = repository.Count();
            Assert.Equal(53, count); // 50 seeded + 3 new
        }

        #endregion

        #region ExecuteUpdate Tests (Async)

        [Fact]
        public async Task UpdateAsync_ShouldUpdateMatchingBlogs()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update all blogs with TypeId = 1 to have a new title prefix
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.TypeId == 1,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, b => "Updated: " + b.Title)
            );

            // Assert
            Assert.True(updatedCount > 0);

            // Verify the updates
            var updatedBlogs = await repository.SearchAsync(
                repository.MultipleResultQuery().AndFilter(b => b.TypeId == 1)
            );

            Assert.All(updatedBlogs, blog => Assert.StartsWith("Updated: ", blog.Title));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSingleProperty()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            var targetBlogId = 1;

            // Act - Update URL for a specific blog
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.Id == targetBlogId,
                setPropertyCalls: setters => setters.SetProperty(b => b.Url, "/updated-url-async")
            );

            // Assert
            Assert.Equal(1, updatedCount);

            // Verify the update
            var updatedBlog = await repository.FirstOrDefaultAsync(
                repository.SingleResultQuery().AndFilter(b => b.Id == targetBlogId)
            );

            Assert.Equal("/updated-url-async", updatedBlog.Url);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateMultipleProperties()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update multiple properties for blogs with TypeId = 2
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.TypeId == 2,
                setPropertyCalls: setters => setters
                    .SetProperty(b => b.Title, "Bulk Updated Title Async")
                    .SetProperty(b => b.Url, "/bulk-updated-async")
            );

            // Assert
            Assert.True(updatedCount > 0);

            // Verify the updates
            var updatedBlogs = await repository.SearchAsync(
                repository.MultipleResultQuery().AndFilter(b => b.TypeId == 2)
            );

            Assert.All(updatedBlogs, blog =>
            {
                Assert.Equal("Bulk Updated Title Async", blog.Title);
                Assert.Equal("/bulk-updated-async", blog.Url);
            });
        }

        [Fact]
        public async Task UpdateAsync_WithNoMatches_ShouldReturnZero()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Try to update blogs that don't exist
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.Id == 99999,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, "Should Not Update")
            );

            // Assert
            Assert.Equal(0, updatedCount);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateBasedOnComputedValue()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update title by appending existing URL
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.Id <= 5,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, b => b.Title + " [" + b.Url + "]")
            );

            // Assert
            Assert.Equal(5, updatedCount);

            // Verify
            var updatedBlogs = await repository.SearchAsync(
                repository.MultipleResultQuery().AndFilter(b => b.Id <= 5)
            );

            Assert.All(updatedBlogs, blog => Assert.Contains("[", blog.Title));
        }

        #endregion

        #region ExecuteUpdate Tests (Sync)

        [Fact]
        public void Update_ShouldUpdateMatchingBlogs_Sync()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update all blogs with TypeId = 1 to have a new title prefix
            var updatedCount = repository.Update(
                predicate: blog => blog.TypeId == 1,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, b => "Sync Updated: " + b.Title)
            );

            // Assert
            Assert.True(updatedCount > 0);

            // Verify the updates
            var updatedBlogs = repository.Search(
                repository.MultipleResultQuery().AndFilter(b => b.TypeId == 1)
            );

            Assert.All(updatedBlogs, blog => Assert.StartsWith("Sync Updated: ", blog.Title));
        }

        [Fact]
        public void Update_ShouldUpdateSingleProperty_Sync()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            var targetBlogId = 2;

            // Act - Update URL for a specific blog
            var updatedCount = repository.Update(
                predicate: blog => blog.Id == targetBlogId,
                setPropertyCalls: setters => setters.SetProperty(b => b.Url, "/updated-url-sync")
            );

            // Assert
            Assert.Equal(1, updatedCount);

            // Verify the update
            var updatedBlog = repository.FirstOrDefault(
                repository.SingleResultQuery().AndFilter(b => b.Id == targetBlogId)
            );

            Assert.Equal("/updated-url-sync", updatedBlog.Url);
        }

        [Fact]
        public void Update_ShouldUpdateMultipleProperties_Sync()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update multiple properties for blogs with TypeId = 2
            var updatedCount = repository.Update(
                predicate: blog => blog.TypeId == 2,
                setPropertyCalls: setters => setters
                    .SetProperty(b => b.Title, "Bulk Updated Title Sync")
                    .SetProperty(b => b.Url, "/bulk-updated-sync")
            );

            // Assert
            Assert.True(updatedCount > 0);

            // Verify the updates
            var updatedBlogs = repository.Search(
                repository.MultipleResultQuery().AndFilter(b => b.TypeId == 2)
            );

            Assert.All(updatedBlogs, blog =>
            {
                Assert.Equal("Bulk Updated Title Sync", blog.Title);
                Assert.Equal("/bulk-updated-sync", blog.Url);
            });
        }

        [Fact]
        public void Update_WithNoMatches_ShouldReturnZero_Sync()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Try to update blogs that don't exist
            var updatedCount = repository.Update(
                predicate: blog => blog.Id == 99999,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, "Should Not Update")
            );

            // Assert
            Assert.Equal(0, updatedCount);
        }

        #endregion

        #region Entity-Based Update Tests

        [Fact]
        public void UpdateEntity_ShouldUpdateSpecificProperties()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            
            var blog = repository.FirstOrDefault(repository.SingleResultQuery().AndFilter(b => b.Id == 3));
            
            // Modify the blog
            blog.Title = "Updated via Entity";
            blog.Url = "/updated-via-entity";

            // Act - Update only specific properties
            var updatedBlog = repository.Update(blog, b => b.Title, b => b.Url);
            unitOfWork.SaveChanges();

            // Assert
            Assert.NotNull(updatedBlog);
            
            // Reload from database to verify
            var reloadedBlog = repository.FirstOrDefault(
                repository.SingleResultQuery().AndFilter(b => b.Id == 3)
            );
            Assert.Equal("Updated via Entity", reloadedBlog.Title);
            Assert.Equal("/updated-via-entity", reloadedBlog.Url);
        }

        [Fact]
        public async Task UpdateEntity_ShouldUpdateAllProperties_Async()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            
            var blog = await repository.FirstOrDefaultAsync(
                repository.SingleResultQuery().AndFilter(b => b.Id == 4)
            );
            
            // Modify the blog
            blog.Title = "Fully Updated Blog";
            blog.Url = "/fully-updated";
            blog.TypeId = 2;

            // Act - Update all properties (no property expressions)
            var updatedBlog = repository.Update(blog);
            await unitOfWork.SaveChangesAsync();

            // Assert
            var reloadedBlog = await repository.FirstOrDefaultAsync(
                repository.SingleResultQuery().AndFilter(b => b.Id == 4)
            );
            Assert.Equal("Fully Updated Blog", reloadedBlog.Title);
            Assert.Equal("/fully-updated", reloadedBlog.Url);
            Assert.Equal(2, reloadedBlog.TypeId);
        }

        #endregion
    }
}
