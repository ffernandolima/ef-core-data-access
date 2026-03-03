using EntityFrameworkCore.Models;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
//   - A real database ensures bulk update operations and persistence work correctly
//
// To run these tests:
//   dotnet test --filter "FullyQualifiedName~DataAccessIntegrationTests"
//
// To skip these tests (run only in-memory tests):
//   dotnet test --filter "FullyQualifiedName!~DataAccessIntegrationTests"
// ========================================================================================================

namespace EntityFrameworkCore.Tests
{
    /// <summary>
    /// Integration tests for the data access layer (Unit of Work + Repository) using PostgreSQL via Testcontainers.
    /// These tests verify the full stack including ExecuteUpdate/ExecuteUpdateAsync, which the In-Memory provider does not support.
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
    /// dotnet test --filter FullyQualifiedName~DataAccessIntegrationTests
    /// </code>
    /// <para>
    /// If Docker is not available, these tests will be skipped automatically with a clear error message.
    /// The standard in-memory tests in <see cref="AsyncDataAccessTests"/> and <see cref="SyncDataAccessTests"/> 
    /// will still run without Docker.
    /// </para>
    /// </remarks>
    [Collection("PostgreSql")]
    public class DataAccessIntegrationTests
    {
        private readonly PostgreSqlFixture _fixture;

        public DataAccessIntegrationTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
        }

        private void SkipIfDockerUnavailable() => Skip.If(_fixture.SkipReason is not null, _fixture.SkipReason ?? "Docker unavailable");

        #region Add/Insert Tests

        [SkippableFact]
        public async Task AddAsync_ShouldAddNewBlog()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

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

        [SkippableFact]
        public async Task AddRangeAsync_ShouldAddMultipleBlogs()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            var uniqueSuffix = Guid.NewGuid().ToString("N");
            var newBlogs = new List<Blog>
            {
                new() { Title = $"Integration Blog 1 {uniqueSuffix}", Url = $"/integration-blog-1-{uniqueSuffix}", TypeId = 1 },
                new() { Title = $"Integration Blog 2 {uniqueSuffix}", Url = $"/integration-blog-2-{uniqueSuffix}", TypeId = 2 },
                new() { Title = $"Integration Blog 3 {uniqueSuffix}", Url = $"/integration-blog-3-{uniqueSuffix}", TypeId = 1 }
            };

            // Act
            await repository.AddRangeAsync(newBlogs);
            await unitOfWork.SaveChangesAsync();

            // Assert
            foreach (var blog in newBlogs)
            {
                var exists = await repository.AnyAsync(b => b.Title == blog.Title && b.Url == blog.Url);
                Assert.True(exists, $"Blog with Title '{blog.Title}' and Url '{blog.Url}' should exist.");
            }
        }

        [SkippableFact]
        public void Add_ShouldAddNewBlog_Sync()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            var uniqueSuffix = Guid.NewGuid().ToString("N");
            var newBlog = new Blog
            {
                Title = $"New Sync Integration Blog {uniqueSuffix}",
                Url = $"/new-sync-integration-blog-{uniqueSuffix}",
                TypeId = 1
            };

            // Act
            var addedBlog = repository.Add(newBlog);
            unitOfWork.SaveChanges();

            // Assert
            Assert.NotNull(addedBlog);
            Assert.Equal(newBlog.Title, addedBlog.Title);
            Assert.True(addedBlog.Id > 0);

            var exists = repository.Any(b => b.Title == newBlog.Title && b.Url == newBlog.Url);
            Assert.True(exists, $"Blog with Title '{newBlog.Title}' and Url '{newBlog.Url}' should exist.");
        }

        [SkippableFact]
        public void AddRange_ShouldAddMultipleBlogs_Sync()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            var uniqueSuffix = Guid.NewGuid().ToString("N");
            var newBlogs = new List<Blog>
            {
                new() { Title = $"Sync Integration Blog 1 {uniqueSuffix}", Url = $"/sync-int-blog-1-{uniqueSuffix}", TypeId = 1 },
                new() { Title = $"Sync Integration Blog 2 {uniqueSuffix}", Url = $"/sync-int-blog-2-{uniqueSuffix}", TypeId = 2 },
                new() { Title = $"Sync Integration Blog 3 {uniqueSuffix}", Url = $"/sync-int-blog-3-{uniqueSuffix}", TypeId = 1 }
            };

            // Act
            repository.AddRange(newBlogs);
            unitOfWork.SaveChanges();

            // Assert
            foreach (var blog in newBlogs)
            {
                var exists = repository.Any(b => b.Title == blog.Title && b.Url == blog.Url);
                Assert.True(exists, $"Blog with Title '{blog.Title}' and Url '{blog.Url}' should exist.");
            }
        }

        #endregion Add/Insert Tests

        #region ExecuteUpdate Tests (Async)

        [SkippableFact]
        public async Task UpdateAsync_ShouldUpdateMatchingBlogs()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update all blogs with TypeId = 1 to have a new title prefix
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.TypeId == 1,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, b => "Updated: " + b.Title));

            // Assert
            Assert.True(updatedCount > 0);

            // Verify the updates
            var updatedBlogs = await repository.SearchAsync(
                repository.MultipleResultQuery().AndFilter(b => b.TypeId == 1));

            Assert.All(updatedBlogs, blog => Assert.StartsWith("Updated: ", blog.Title));
        }

        [SkippableFact]
        public async Task UpdateAsync_ShouldUpdateSingleProperty()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            var targetBlogId = 1;

            // Act - Update URL for a specific blog
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.Id == targetBlogId,
                setPropertyCalls: setters => setters.SetProperty(b => b.Url, "/updated-url-async"));

            // Assert
            Assert.Equal(1, updatedCount);

            // Verify the update
            var updatedBlog = await repository.FirstOrDefaultAsync(
                repository.SingleResultQuery().AndFilter(b => b.Id == targetBlogId));

            Assert.Equal("/updated-url-async", updatedBlog.Url);
        }

        [SkippableFact]
        public async Task UpdateAsync_ShouldUpdateMultipleProperties()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update multiple properties for blogs with TypeId = 2
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.TypeId == 2,
                setPropertyCalls: setters => setters
                    .SetProperty(b => b.Title, "Bulk Updated Title Async")
                    .SetProperty(b => b.Url, "/bulk-updated-async"));

            // Assert
            Assert.True(updatedCount > 0);

            // Verify the updates
            var updatedBlogs = await repository.SearchAsync(
                repository.MultipleResultQuery().AndFilter(b => b.TypeId == 2));

            Assert.All(updatedBlogs, blog =>
            {
                Assert.Equal("Bulk Updated Title Async", blog.Title);
                Assert.Equal("/bulk-updated-async", blog.Url);
            });
        }

        [SkippableFact]
        public async Task UpdateAsync_WithNoMatches_ShouldReturnZero()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Try to update blogs that don't exist
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.Id == 99999,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, "Should Not Update"));

            // Assert
            Assert.Equal(0, updatedCount);
        }

        [SkippableFact]
        public async Task UpdateAsync_ShouldUpdateBasedOnComputedValue()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update title by appending existing URL
            var updatedCount = await repository.UpdateAsync(
                predicate: blog => blog.Id <= 5,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, b => b.Title + " [" + b.Url + "]"));

            // Assert
            Assert.Equal(5, updatedCount);

            // Verify
            var updatedBlogs = await repository.SearchAsync(
                repository.MultipleResultQuery().AndFilter(b => b.Id <= 5));

            Assert.All(updatedBlogs, blog => Assert.Contains("[", blog.Title));
        }

        #endregion ExecuteUpdate Tests (Async)

        #region ExecuteUpdate Tests (Sync)

        [SkippableFact]
        public void Update_ShouldUpdateMatchingBlogs_Sync()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update all blogs with TypeId = 1 to have a new title prefix
            var updatedCount = repository.Update(
                predicate: blog => blog.TypeId == 1,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, b => "Sync Updated: " + b.Title));

            // Assert
            Assert.True(updatedCount > 0);

            // Verify the updates
            var updatedBlogs = repository.Search(
                repository.MultipleResultQuery().AndFilter(b => b.TypeId == 1));

            Assert.All(updatedBlogs, blog => Assert.StartsWith("Sync Updated: ", blog.Title));
        }

        [SkippableFact]
        public void Update_ShouldUpdateSingleProperty_Sync()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();
            var targetBlogId = 2;

            // Act - Update URL for a specific blog
            var updatedCount = repository.Update(
                predicate: blog => blog.Id == targetBlogId,
                setPropertyCalls: setters => setters.SetProperty(b => b.Url, "/updated-url-sync"));

            // Assert
            Assert.Equal(1, updatedCount);

            // Verify the update
            var updatedBlog = repository.FirstOrDefault(
                repository.SingleResultQuery().AndFilter(b => b.Id == targetBlogId));

            Assert.Equal("/updated-url-sync", updatedBlog.Url);
        }

        [SkippableFact]
        public void Update_ShouldUpdateMultipleProperties_Sync()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Update multiple properties for blogs with TypeId = 2
            var updatedCount = repository.Update(
                predicate: blog => blog.TypeId == 2,
                setPropertyCalls: setters => setters
                    .SetProperty(b => b.Title, "Bulk Updated Title Sync")
                    .SetProperty(b => b.Url, "/bulk-updated-sync"));

            // Assert
            Assert.True(updatedCount > 0);

            // Verify the updates
            var updatedBlogs = repository.Search(
                repository.MultipleResultQuery().AndFilter(b => b.TypeId == 2));

            Assert.All(updatedBlogs, blog =>
            {
                Assert.Equal("Bulk Updated Title Sync", blog.Title);
                Assert.Equal("/bulk-updated-sync", blog.Url);
            });
        }

        [SkippableFact]
        public void Update_WithNoMatches_ShouldReturnZero_Sync()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            // Act - Try to update blogs that don't exist
            var updatedCount = repository.Update(
                predicate: blog => blog.Id == 99999,
                setPropertyCalls: setters => setters.SetProperty(b => b.Title, "Should Not Update"));

            // Assert
            Assert.Equal(0, updatedCount);
        }

        #endregion ExecuteUpdate Tests (Sync)

        #region Entity-Based Update Tests

        [SkippableFact]
        public void UpdateEntity_ShouldUpdateSpecificProperties()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

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

            // Reload from database via new scope to verify persistence (avoids EF returning tracked instance)
            using var verifyScope = _fixture.ServiceProvider!.CreateScope();

            var verifyRepository = verifyScope.ServiceProvider.GetRequiredService<IUnitOfWork>().Repository<Blog>();
            var reloadedBlog = verifyRepository.FirstOrDefault(verifyRepository.SingleResultQuery().AndFilter(b => b.Id == 3));

            Assert.Equal("Updated via Entity", reloadedBlog.Title);
            Assert.Equal("/updated-via-entity", reloadedBlog.Url);
        }

        [SkippableFact]
        public async Task UpdateEntity_ShouldUpdateAllProperties_Async()
        {
            SkipIfDockerUnavailable();

            // Arrange
            using var scope = _fixture.ServiceProvider!.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<Blog>();

            var blog = await repository.FirstOrDefaultAsync(repository.SingleResultQuery().AndFilter(b => b.Id == 4));

            // Modify the blog
            blog.Title = "Fully Updated Blog";
            blog.Url = "/fully-updated";
            blog.TypeId = 2;

            // Act - Update all properties (no property expressions)
            var updatedBlog = repository.Update(blog);
            await unitOfWork.SaveChangesAsync();

            // Assert - reload via new scope to verify persistence (avoids EF returning tracked instance)
            using var verifyScope = _fixture.ServiceProvider!.CreateScope();

            var verifyRepository = verifyScope.ServiceProvider.GetRequiredService<IUnitOfWork>().Repository<Blog>();
            var reloadedBlog = await verifyRepository.FirstOrDefaultAsync(verifyRepository.SingleResultQuery().AndFilter(b => b.Id == 4));

            Assert.Equal("Fully Updated Blog", reloadedBlog.Title);
            Assert.Equal("/fully-updated", reloadedBlog.Url);
            Assert.Equal(2, reloadedBlog.TypeId);
        }

        #endregion Entity-Based Update Tests
    }
}
