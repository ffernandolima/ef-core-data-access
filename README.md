# EntityFrameworkCore.DataAccess

It's a modern and generic data access structure for .NET and Microsoft.EntityFrameworkCore. It supports UnitOfWork, Repository and QueryBuilder patterns. It also includes multiple databases support with distributed transactions and databases/tables sharding for some database providers.

 | Package | NuGet |
 | ------- | ----- |
 | EntityFrameworkCore.Data.QueryBuilder | [![Nuget](https://img.shields.io/badge/nuget-v2.0.2-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.QueryBuilder)](https://www.nuget.org/packages/EntityFrameworkCore.Data.QueryBuilder/2.0.2) |
 | EntityFrameworkCore.Data.Repository | [![Nuget](https://img.shields.io/badge/nuget-v2.0.2-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.Repository)](https://www.nuget.org/packages/EntityFrameworkCore.Data.Repository/2.0.2) |
 | EntityFrameworkCore.Data.UnitOfWork | [![Nuget](https://img.shields.io/badge/nuget-v2.0.2-blue) ![Nuget](https://img.shields.io/nuget/dt/EntityFrameworkCore.Data.UnitOfWork)](https://www.nuget.org/packages/EntityFrameworkCore.Data.UnitOfWork/2.0.2) |

## Installation

EntityFrameworkCore.DataAccess is available on Nuget.

```
Install-Package EntityFrameworkCore.Data.QueryBuilder -Version 2.0.2
Install-Package EntityFrameworkCore.Data.Repository -Version 2.0.2
Install-Package EntityFrameworkCore.Data.UnitOfWork -Version 2.0.2
```

P.S.: EntityFrameworkCore.Data.UnitOfWork depends on the other two packages, so installing this package is enough.

## Usage

The following code demonstrates basic usage.

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

Please check some available samples [here](https://github.com/ffernandolima/ef-core-data-access/tree/ef-core-2/samples)

## Support / Contributing
If you want to help with the project, feel free to open pull requests and submit issues. 

## Donate

If you would like to show your support for this project, then please feel free to buy me a coffee.

<a href="https://www.buymeacoffee.com/fernandolima" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/white_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>


