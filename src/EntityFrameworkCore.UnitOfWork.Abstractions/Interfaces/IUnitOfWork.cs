using Microsoft.EntityFrameworkCore;
using System;

namespace EntityFrameworkCore.UnitOfWork.Interfaces
{
    public interface IUnitOfWork : ISyncUnitOfWork, IAsyncUnitOfWork, IDisposable
    {
        DbContext DbContext { get; }
        TimeSpan? Timeout { get; set; }
    }

    public interface IUnitOfWork<T> : IUnitOfWork, ISyncUnitOfWork<T>, IAsyncUnitOfWork<T>, IDisposable where T : DbContext
    { }
}
