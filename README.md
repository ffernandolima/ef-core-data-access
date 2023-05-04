# EntityFrameworkCore.DataAccess

It's a modern and generic data access structure for .NET and Microsoft.EntityFrameworkCore. It supports UnitOfWork, Repository and QueryBuilder patterns. It also includes auto history utilities, multiple databases support with distributed transactions and databases/tables sharding for some database providers.

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Status

[![build-and-tests Workflow Status](https://github.com/ffernandolima/ef-core-data-access/actions/workflows/build-and-tests.yml/badge.svg?branch=ef-core-7)](https://github.com/ffernandolima/ef-core-data-access/actions/workflows/build-and-tests.yml?branch=ef-core-7)

[![build-and-publish Workflow Status](https://github.com/ffernandolima/ef-core-data-access/actions/workflows/build-and-publish.yml/badge.svg?branch=ef-core-7)](https://github.com/ffernandolima/ef-core-data-access/actions/workflows/build-and-publish.yml?branch=ef-core-7)

 | Package | NuGet |
 | ------- | ------- |
 | EntityFrameworkCore.Data.QueryBuilder.Abstractions | [![Nuget](https://img.shields.io/badge/nuget-v7.0.7-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.QueryBuilder.Abstractions)](https://www.nuget.org/packages/EntityFrameworkCore.Data.QueryBuilder.Abstractions/7.0.7) |
 | EntityFrameworkCore.Data.Repository.Abstractions | [![Nuget](https://img.shields.io/badge/nuget-v7.0.7-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.Repository.Abstractions)](https://www.nuget.org/packages/EntityFrameworkCore.Data.Repository.Abstractions/7.0.7) |
 | EntityFrameworkCore.Data.UnitOfWork.Abstractions | [![Nuget](https://img.shields.io/badge/nuget-v7.0.7-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.UnitOfWork.Abstractions)](https://www.nuget.org/packages/EntityFrameworkCore.Data.UnitOfWork.Abstractions/7.0.7) |
 | ------- | ------- |
 | EntityFrameworkCore.Data.AutoHistory | [![Nuget](https://img.shields.io/badge/nuget-v7.0.7-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.AutoHistory)](https://www.nuget.org/packages/EntityFrameworkCore.Data.AutoHistory/7.0.7) |
 | EntityFrameworkCore.Data.QueryBuilder | [![Nuget](https://img.shields.io/badge/nuget-v7.0.7-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.QueryBuilder)](https://www.nuget.org/packages/EntityFrameworkCore.Data.QueryBuilder/7.0.7) |
 | EntityFrameworkCore.Data.Repository | [![Nuget](https://img.shields.io/badge/nuget-v7.0.7-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.Repository)](https://www.nuget.org/packages/EntityFrameworkCore.Data.Repository/7.0.7) |
 | EntityFrameworkCore.Data.UnitOfWork | [![Nuget](https://img.shields.io/badge/nuget-v7.0.7-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.UnitOfWork)](https://www.nuget.org/packages/EntityFrameworkCore.Data.UnitOfWork/7.0.7) |

## Installation

EntityFrameworkCore.DataAccess is available on Nuget.

```
Install-Package EntityFrameworkCore.Data.QueryBuilder.Abstractions -Version 7.0.7
Install-Package EntityFrameworkCore.Data.Repository.Abstractions -Version 7.0.7
Install-Package EntityFrameworkCore.Data.UnitOfWork.Abstractions -Version 7.0.7

Install-Package EntityFrameworkCore.Data.AutoHistory -Version 7.0.7
Install-Package EntityFrameworkCore.Data.QueryBuilder -Version 7.0.7
Install-Package EntityFrameworkCore.Data.Repository -Version 7.0.7
Install-Package EntityFrameworkCore.Data.UnitOfWork -Version 7.0.7
```

P.S.: EntityFrameworkCore.Data.UnitOfWork depends on the other packages, so installing this package is enough.

## Usage

#### The following code demonstrates basic usage of UnitOfWork, Repository and QueryBuilder patterns.

First of all, please register the dependencies into the MS Built-In container:

```C#
var connectionString = Configuration.GetConnectionString("EFCoreDataAccess"); // Use your connection string name

services.AddDbContext<BloggingContext>(options =>
{
    // This example uses SQL Server. Use your provider here
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        var assembly = typeof(BloggingContext).Assembly;
        var assemblyName = assembly.GetName();

        sqlServerOptions.MigrationsAssembly(assemblyName.Name);
    });
 
    options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
});

// Register the DbContext instance
services.AddScoped<DbContext, BloggingContext>();

// Register the UnitOfWork
services.AddUnitOfWork(); 
services.AddUnitOfWork<BloggingContext>(); // Multiple databases support

```

After that, use the structure in your code like that:

```C#
private readonly IUnitOfWork _unitOfWork;
	
// Injection
public BlogsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

public void GetAllBlogs()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.MultipleResultQuery()
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

    var blogs = repository.Search(query);
}


public void GetAllBlogsProjection()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.MultipleResultQuery()
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments))
                          .Select(selector => new { Name = selector.Title, Link = selector.Url, Type = selector.Type.Description });

    var blogs = repository.Search(query);
}


public void GetAllOrderedBlogs()
{
    var repository = _unitOfWork.Repository<Blog>();

    IQuery<Blog> query = null;
    IList<Blog> blogs = null;

    query = repository.MultipleResultQuery()
                      .OrderByDescending("Type.Id")
                      .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

    blogs = repository.Search(query);

    query = repository.MultipleResultQuery()
                      .OrderByDescending(blog => blog.Type.Id)
                      .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

    blogs = repository.Search(query);
}


public void GetTopBlogs()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.MultipleResultQuery()
                          .Top(10)
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

    var blogs = repository.Search(query);
}


public void GetPagedBlogs()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.MultipleResultQuery()
                          .Page(1, 20)
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments)) as IMultipleResultQuery<Blog>;

    var blogs = repository.Search(query);
}


public void GetBlogsPagedList()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.MultipleResultQuery()
                          .Page(1, 20)
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments)) as IMultipleResultQuery<Blog>;

    var blogs = repository.Search(query)
                          .ToPagedList(query.Paging.PageIndex, query.Paging.PageSize, query.Paging.TotalCount);
}


public void GetFilteredBlogs()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.MultipleResultQuery()
                          .AndFilter(blog => blog.Url.StartsWith("/a/"))
                          .AndFilter(blog => blog.Title.StartsWith("a"))
                          .AndFilter(blog => blog.Posts.Any())
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

    var blogs = repository.Search(query);
}

public void GetUrls()
{    
    var repository = _unitOfWork.CustomRepository<ICustomBlogRepository>();

    var urls = repository.GetAllBlogUrls();
}

public void GetBlogByUrl()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.SingleResultQuery()
                          .AndFilter(blog => blog.Url.StartsWith("/a/"))
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments))
                          .OrderByDescending(blog => blog.Id);

    var blogResult = repository.FirstOrDefault(query);
}


public void GetBlogById()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.SingleResultQuery()
                          .AndFilter(blog => blog.Id == 1)
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

    var blogResult = repository.SingleOrDefault(query);
}


public void GetBlogByIdProjection()
{
    var repository = _unitOfWork.Repository<Blog>();

    var query = repository.SingleResultQuery()
                          .AndFilter(blog => blog.Id == 1)
                          .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments))
                          .Select(selector => new { selector.Id, Name = selector.Title, Link = selector.Url, Type = selector.Type.Description });

    var blogResult = repository.SingleOrDefault(query);
}


public void ExistsBlog()
{
    var repository = _unitOfWork.Repository<Blog>();

    var exists = repository.Any(blog => blog.Url.StartsWith("/a/"));
}


public void GetBlogCount()
{
    var repository = _unitOfWork.Repository<Blog>();

    var count = repository.Count();

    var longCount = repository.LongCount();
}


public void MaxBlogId()
{
    var repository = _unitOfWork.Repository<Blog>();

    var id = repository.Max(blog => blog.Id);
}


public void MinBlogId()
{
    var repository = _unitOfWork.Repository<Blog>();

    var id = repository.Min(blog => blog.Id);
}


public void AddBlog()
{
    var repository = _unitOfWork.Repository<Blog>();

    var blog = repository.Add(Seeder.SeedBlog(51));

    _unitOfWork.SaveChanges();
}

public void UpdateBlog()
{
    var repository = _unitOfWork.Repository<Blog>();

    var blog = repository.Update(model);
    
    _unitOfWork.SaveChanges();
}

public void DeleteBlog()
{
    var repository = _unitOfWork.Repository<Blog>();

    repository.Remove(x => x.Id == id);
    
    repository.Remove(model);
    
    _unitOfWork.SaveChanges();
}

```

The operations above are also available as async.

Please check some available samples [here](https://github.com/ffernandolima/ef-core-data-access/tree/ef-core-7/samples)


####  The following code demonstrates basic usage of AutoHistory utilities

```AutoHistory``` will save all entity changes into a table named ```AutoHistories```. By default, it saves ```Modified``` and ```Deleted``` entities.

First of all, please enable ```AutoHistory``` through a ```ModelBuilder``` instance:

```C#

public class BloggingContext : DbContext
{
    public BloggingContext(DbContextOptions<BloggingContext> options)
       : base(options)
    {
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder builder) 
    {
        // Enables auto history functionality.
        builder.EnableAutoHistory();
        builder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly); 
    }
}
```

After that, to ensure that entity changes will be saved automatically, the method ```EnsureAutoHistory()``` must be called before ```SaveChanges()``` or ```SaveChangesAsync()```. It can be done globally by overriding ```SaveChanges()``` and ```SaveChangesAsync()```:


```C#

public class BloggingContext : DbContext
{
    public BloggingContext(DbContextOptions<BloggingContext> options)
       : base(options)
    {
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public override int SaveChanges()
    {
        this.EnsureAutoHistory();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        this.EnsureAutoHistory();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder) 
    {
        // Enables auto history functionality.
        builder.EnableAutoHistory();
        builder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly); 
    }
}
```

PS.: By default, it saves ```Modified``` and ```Deleted``` entities.

```Added``` entities can be saved as well by changing the default behaviour:

```C#

public class BloggingContext : DbContext
{
    public BloggingContext(DbContextOptions<BloggingContext> options)
       : base(options)
    {
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public override int SaveChanges()
    {
        var addedEntities = this.DetectChanges(EntityState.Added);

        this.EnsureAutoHistory();
        var affectedRows = base.SaveChanges();

        this.EnsureAutoHistory(addedEntities);
        affectedRows += base.SaveChanges();

        return affectedRows;
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var addedEntities = this.DetectChanges(EntityState.Added);

        this.EnsureAutoHistory();
        var affectedRows = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.EnsureAutoHistory(addedEntities);
        affectedRows += await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return affectedRows;
    }

    protected override void OnModelCreating(ModelBuilder builder) 
    {
        // Enables auto history functionality.
        builder.EnableAutoHistory();
        builder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly); 
    }
}
```

```Added``` entities should be handled in a different step to ensure that the auto-increment PK values have already been generated by EF Core/Database before.

##### Custom AutoHistory Entity

A custom auto history entity can be used by extending the ```AutoHistory``` class:

```C#

public class CustomAutoHistory : AutoHistory
{
    public String CustomField { get; set; }
}

```

Then register it into through a ```ModelBuilder``` instance:

```C#

modelBuilder.EnableAutoHistory<CustomAutoHistory>();


```

Then provide a custom history entity factory when calling ```EnsureAutoHistory()```:

```C#

this.EnsureAutoHistory(() => new CustomAutoHistory { CustomField = "CustomValue" });

```

##### Excluding entities from AutoHistory

Entities can be excluded from being serialized and saved into the ```AutoHistory``` table by adding ```[ExcludeFromHistoryAttribute]``` attribute directly to the entity:

```C#
[ExcludeFromHistory]
public class Blog
{
    public string PrivateURL { get; set; }
}
```

##### Excluding properties from AutoHistory

Properties can be excluded from being serialized and saved into the ```AutoHistory``` table by adding ```[ExcludeFromHistoryAttribute]``` attribute to model properties:

```C#
public class Blog
{        
    [ExcludeFromHistory]
    public string PrivateURL { get; set; }
}
```

## Support / Contributing
If you want to help with the project, feel free to open pull requests and submit issues. 

## Donate

If you would like to show your support for this project, then please feel free to buy me a coffee.

<a href="https://www.buymeacoffee.com/fernandolima" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/white_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>
