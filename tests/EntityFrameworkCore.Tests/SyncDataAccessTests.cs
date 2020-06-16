using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Extensions;
using EntityFrameworkCore.UnitOfWork.Extensions;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EntityFrameworkCore.Tests
{
    public class SyncDataAccessTests : Startup
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWork<BloggingContext> _unitOfWorkOfT;

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRepositoryFactory<BloggingContext> _repositoryFactoryOfT;

        public SyncDataAccessTests()
            : base()
        {
            // IUnitOfWork used for reading/writing scenario;
            _unitOfWork = ServiceProvider.GetService<IUnitOfWork>();
            // IUnitOfWork<T> used for used for multiple databases scenario;
            _unitOfWorkOfT = ServiceProvider.GetService<IUnitOfWork<BloggingContext>>();

            // IRepositoryFactory used for readonly scenario;
            _repositoryFactory = ServiceProvider.GetService<IRepositoryFactory>();
            // IRepositoryFactory<T> used for readonly/multiple databases scenario;
            _repositoryFactoryOfT = ServiceProvider.GetService<IRepositoryFactory<BloggingContext>>();

            Seed();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BloggingContext>(options => options.UseInMemoryDatabase(nameof(BloggingContext)), ServiceLifetime.Transient);
            services.AddTransient<DbContext, BloggingContext>();
            services.AddUnitOfWork();
            services.AddUnitOfWork<BloggingContext>();
        }

        [Fact]
        public void GetAllBlogs()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.MultipleResultQuery()
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

            var blogs = repository.Search(query);

            Assert.NotNull(blogs);
            Assert.Equal(50, blogs.Count);
        }

        [Fact]
        public void GetAllBlogsProjection()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.MultipleResultQuery()
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments))
                                  .Select(selector => new { Name = selector.Title, Link = selector.Url, Type = selector.Type.Description });

            var blogs = repository.Search(query);

            Assert.NotNull(blogs);
            Assert.Equal(50, blogs.Count);
            Assert.Equal("a1", blogs[0].Name);
            Assert.Equal("/a/1", blogs[0].Link);
            Assert.Equal("z1", blogs[0].Type);
        }

        [Fact]
        public void GetAllOrderedBlogs()
        {
            var repository = _unitOfWork.Repository<Blog>();

            IQuery<Blog> query = null;
            IList<Blog> blogs = null;

            query = repository.MultipleResultQuery()
                              .OrderByDescending("Type.Id")
                              .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

            blogs = repository.Search(query);

            Assert.NotNull(blogs);
            Assert.Equal(50, blogs.Count);
            Assert.Equal(50, blogs[0].Id);

            query = repository.MultipleResultQuery()
                              .OrderByDescending(blog => blog.Type.Id)
                              .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

            blogs = repository.Search(query);

            Assert.NotNull(blogs);
            Assert.Equal(50, blogs.Count);
            Assert.Equal(50, blogs[0].Id);
        }

        [Fact]
        public void GetTopBlogs()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.MultipleResultQuery()
                                  .Top(10)
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

            var blogs = repository.Search(query);

            Assert.NotNull(blogs);
            Assert.Equal(10, blogs.Count);
        }

        [Fact]
        public void GetPagedBlogs()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.MultipleResultQuery()
                                  .Page(1, 20)
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments)) as IMultipleResultQuery<Blog>;

            var blogs = repository.Search(query);

            Assert.NotNull(blogs);
            Assert.Equal(20, blogs.Count);
            Assert.Equal(1, query.Paging.PageIndex);
            Assert.Equal(20, query.Paging.PageSize);
            Assert.Equal(50, query.Paging.TotalCount);
        }

        [Fact]
        public void GetBlogsPagedList()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.MultipleResultQuery()
                                  .Page(1, 20)
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments)) as IMultipleResultQuery<Blog>;

            var blogs = repository.Search(query)
                                  .ToPagedList(query.Paging.PageIndex, query.Paging.PageSize, query.Paging.TotalCount);

            Assert.NotNull(blogs);
            Assert.Equal(20, blogs.Count);
            Assert.Equal(1, blogs.PageIndex);
            Assert.Equal(20, blogs.PageSize);
            Assert.Equal(50, blogs.TotalCount);
            Assert.Equal(3, blogs.TotalPages);
            Assert.False(blogs.HasPreviousPage);
            Assert.True(blogs.HasNextPage);
        }

        [Fact]
        public void GetFilteredBlogs()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.MultipleResultQuery()
                                  .AndFilter(blog => blog.Url.StartsWith("/a/"))
                                  .AndFilter(blog => blog.Title.StartsWith("a"))
                                  .AndFilter(blog => blog.Posts.Any())
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

            var blogs = repository.Search(query);

            Assert.NotNull(blogs);
            Assert.Equal(50, blogs.Count);
        }

        [Fact]
        public void GetBlogByUrl()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.SingleResultQuery()
                                  .AndFilter(blog => blog.Url.StartsWith("/a/"))
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments))
                                  .OrderByDescending(blog => blog.Id);

            var blogResult = repository.FirstOrDefault(query);

            Assert.NotNull(blogResult);
            Assert.Equal(50, blogResult.Id);
            Assert.Equal("/a/50", blogResult.Url);
        }

        [Fact]
        public void GetBlogById()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.SingleResultQuery()
                                  .AndFilter(blog => blog.Id == 1)
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments));

            var blogResult = repository.SingleOrDefault(query);

            Assert.NotNull(blogResult);
            Assert.Equal(1, blogResult.Id);
            Assert.Equal("/a/1", blogResult.Url);
        }

        [Fact]
        public void GetBlogByIdProjection()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var query = repository.SingleResultQuery()
                                  .AndFilter(blog => blog.Id == 1)
                                  .Include(source => source.Include(blog => blog.Type).Include(blog => blog.Posts).ThenInclude(post => post.Comments))
                                  .Select(selector => new { selector.Id, Name = selector.Title, Link = selector.Url, Type = selector.Type.Description });

            var blogResult = repository.SingleOrDefault(query);

            Assert.NotNull(blogResult);
            Assert.Equal(1, blogResult.Id);
            Assert.Equal("a1", blogResult.Name);
            Assert.Equal("/a/1", blogResult.Link);
            Assert.Equal("z1", blogResult.Type);
        }

        [Fact]
        public void ExistsBlog()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var exists = repository.Any(blog => blog.Url.StartsWith("/a/"));

            Assert.True(exists);
        }

        [Fact]
        public void GetBlogCount()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var count = repository.Count();

            Assert.Equal(50, count);

            var longCount = repository.LongCount();

            Assert.Equal(50, longCount);
        }

        [Fact]
        public void MaxBlogId()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var id = repository.Max(blog => blog.Id);

            Assert.Equal(50, id);
        }

        [Fact]
        public void MinBlogId()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var id = repository.Min(blog => blog.Id);

            Assert.Equal(1, id);
        }

        [Fact]
        public void AddBlog()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var blog = repository.Add(Seeder.SeedBlog(51));

            _unitOfWork.SaveChanges();

            Assert.Equal(51, blog.Id);
        }

        private void Seed()
        {
            var repository = _unitOfWork.Repository<Blog>();

            if (!repository.Any())
            {
                var blogs = Seeder.SeedBlogs();

                repository.AddRange(blogs);

                _unitOfWork.SaveChanges();
            }
        }
    }
}
