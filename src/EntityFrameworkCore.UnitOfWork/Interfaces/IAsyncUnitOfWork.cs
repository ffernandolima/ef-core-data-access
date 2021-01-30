using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.UnitOfWork.Interfaces
{
    public interface IAsyncUnitOfWork : IRepositoryFactory, IDisposable
    {
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false, CancellationToken cancellationToken = default);
        Task UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default);
    }

    public interface IAsyncUnitOfWork<T> : IAsyncUnitOfWork, IRepositoryFactory<T>, IDisposable where T : DbContext
    { }
}
