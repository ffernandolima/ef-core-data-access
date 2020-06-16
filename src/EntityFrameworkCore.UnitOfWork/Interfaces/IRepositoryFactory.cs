using EntityFrameworkCore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.UnitOfWork.Interfaces
{
    public interface IRepositoryFactory
    {
        T CustomRepository<T>() where T : class;
        IRepository<T> Repository<T>() where T : class;
    }

    public interface IRepositoryFactory<T> : IRepositoryFactory where T : DbContext
    { }
}
