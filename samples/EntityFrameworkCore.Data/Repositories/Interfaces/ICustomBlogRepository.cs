using EntityFrameworkCore.Models;
using EntityFrameworkCore.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Data.Repositories.Interfaces
{
    public interface ICustomBlogRepository : IRepository<Blog>
    {
        IList<string> GetAllBlogUrls();
        Task<IList<string>> GetAllBlogUrlsAsync(CancellationToken cancellationToken = default);
    }
}
