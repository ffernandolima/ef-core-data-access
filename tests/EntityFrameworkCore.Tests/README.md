# EntityFrameworkCore.Tests

This project contains unit tests and integration tests for the Entity Framework Core Repository pattern implementation.

## Test Types

### 1. In-Memory Unit Tests
- **Files**: `AsyncDataAccessTests.cs`, `SyncDataAccessTests.cs`
- **Database**: EF Core In-Memory Database
- **Requirements**: None - runs anywhere
- **Coverage**: Standard CRUD operations, queries, filtering, paging

### 2. Integration Tests (PostgreSQL + Testcontainers)
- **File**: `RepositoryIntegrationTests.cs`
- **Database**: PostgreSQL 17 (via Docker container)
- **Requirements**: **Docker Desktop must be running**
- **Coverage**: 
  - ExecuteUpdate/ExecuteUpdateAsync operations
  - Bulk updates with property setters
  - Add/AddAsync operations
  - Entity-based updates

## ⚠️ Running Integration Tests

### Prerequisites

**Integration tests require Docker to be installed and running on your local machine.**

1. **Install Docker Desktop**
   - Windows: https://docs.docker.com/desktop/install/windows-install/
   - Mac: https://docs.docker.com/desktop/install/mac-install/
   - Linux: https://docs.docker.com/desktop/install/linux/

2. **Start Docker Desktop**
   - Ensure Docker daemon is running
   - Verify with: `docker ps`

3. **Network Access**
   - Tests will pull `postgres:17-alpine` image (~80MB)
   - Ensure you have internet access for first run

### Running All Tests

```bash
# Run all tests (includes in-memory and integration tests)
dotnet test

# If Docker is not running, integration tests will fail with:
# "Docker is either not running or misconfigured"
```

### Running Only In-Memory Tests

```bash
# Run only tests that don't require Docker
dotnet test --filter "FullyQualifiedName!~RepositoryIntegrationTests"
```

### Running Only Integration Tests

```bash
# Ensure Docker is running first!
docker ps

# Run only integration tests
dotnet test --filter "FullyQualifiedName~RepositoryIntegrationTests"
```

## Why Integration Tests?

**EF Core In-Memory provider limitations:**

The In-Memory database provider does **not support** several EF Core features:
- ❌ `ExecuteUpdate()` / `ExecuteUpdateAsync()` - Bulk updates
- ❌ `ExecuteDelete()` / `ExecuteDeleteAsync()` - Bulk deletes  
- ❌ Raw SQL queries with certain features
- ❌ Database-specific functions

**Integration tests with real PostgreSQL ensure:**
- ✅ Bulk update operations work correctly
- ✅ SQL generation is valid
- ✅ Performance characteristics are realistic
- ✅ Database-specific features function properly

## Test Isolation

Each test uses:
- **Scoped DbContext** - Fresh context per test
- **Isolated database state** - Tests run against a clean PostgreSQL instance so data does not leak between tests
- **Per-test container lifecycle** - Testcontainers starts a new PostgreSQL container for the tests instead of using a long-lived shared instance

## Troubleshooting

### "Docker is either not running or misconfigured"

**Solution**: Start Docker Desktop and verify it's running:
```bash
docker ps
```

### "Cannot connect to Docker daemon"

**Solution**: 
1. Check Docker Desktop is running
2. On Linux, ensure your user is in the `docker` group
3. Verify Docker socket is accessible

### Tests are slow on first run

**Expected**: First run downloads the PostgreSQL image (~80MB). Subsequent runs are much faster as the image is cached.

### Port conflicts

If PostgreSQL port 5432 is already in use, Testcontainers will automatically assign a random available port. No configuration needed.

## Package Dependencies

```xml
<PackageReference Include="Testcontainers.PostgreSql" Version="4.3.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
```

## Continuous Integration (CI/CD)

For CI/CD pipelines (GitHub Actions, Azure Pipelines, etc.):

```yaml
# Example GitHub Actions workflow
- name: Start Docker
  run: docker ps
  
- name: Run Tests
  run: dotnet test
```

Most CI environments have Docker pre-installed and running by default.

## Further Reading

- [Testcontainers for .NET Documentation](https://dotnet.testcontainers.org/)
- [EF Core Testing Documentation](https://learn.microsoft.com/en-us/ef/core/testing/)
- [EF Core 10 Breaking Changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes)
