using EntityFrameworkCore.Data.Repositories.Interfaces;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.Repository;
using EntityFrameworkCore.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Data.Repositories
{
    public class CustomBlogRepository : Repository<Blog>, ICustomBlogRepository
    {
        public CustomBlogRepository(DbContext dbContext)
            : base(dbContext)
        { }

        public IList<string> GetAllBlogUrls()
            => DbContext.Set<Blog>()
                        .Select(blog => blog.Url)
                        .ToList();

        public Task<IList<string>> GetAllBlogUrlsAsync(CancellationToken cancellationToken = default)
            => DbContext.Set<Blog>()
                        .Select(blog => blog.Url)
                        .ToListAsync(cancellationToken)
                        .Then<List<string>, IList<string>>(result => result, cancellationToken);
    }
}
