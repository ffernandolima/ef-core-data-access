using System;

namespace EntityFrameworkCore.Repository.Interfaces
{
    public interface IRepository : IDisposable
    { }

    public interface IRepository<T> : IRepository, ISyncRepository<T>, IAsyncRepository<T>, IDisposable where T : class
    { }
}
