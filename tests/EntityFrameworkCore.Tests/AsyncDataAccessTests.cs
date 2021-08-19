using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.UnitOfWork.Extensions;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace EntityFrameworkCore.Tests
{
    public class AsyncDataAccessTests : Startup
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWork<BloggingContext> _unitOfWorkOfT;

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRepositoryFactory<BloggingContext> _repositoryFactoryOfT;

        public AsyncDataAccessTests()
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

            _ = SeedAsync();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BloggingContext>(options => options.UseInMemoryDatabase(nameof(BloggingContext)), ServiceLifetime.Transient);
            services.AddTransient<DbContext, BloggingContext>();
            services.AddUnitOfWork();
            services.AddUnitOfWork<BloggingContext>();
        }

        [Fact]
        public async Task GetBlogCountAsync()
        {
            var repository = _unitOfWork.Repository<Blog>();

            var count = await repository.CountAsync().ConfigureAwait(continueOnCapturedContext: false);

            Assert.Equal(50, count);

            var longCount = await repository.LongCountAsync().ConfigureAwait(continueOnCapturedContext: false);

            Assert.Equal(50, longCount);
        }

        private async Task SeedAsync()
        {
            var repository = _unitOfWork.Repository<Blog>();

            if (!await repository.AnyAsync().ConfigureAwait(continueOnCapturedContext: false))
            {
                var blogs = Seeder.SeedBlogs();

                await repository.AddRangeAsync(blogs).ConfigureAwait(continueOnCapturedContext: false);

                await _unitOfWork.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }
}
