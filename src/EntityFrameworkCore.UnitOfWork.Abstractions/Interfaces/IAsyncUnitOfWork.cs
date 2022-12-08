using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.UnitOfWork.Interfaces
{
    using System.Data;

    public interface IAsyncUnitOfWork : IRepositoryFactory, IDisposable
    {
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false, CancellationToken cancellationToken = default);
        Task UseTransactionAsync(DbTransaction transaction, Guid? transactionId = null, CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
        Task<IList<T>> FromSqlAsync<T>(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default) where T : class;
        Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default);
    }

    public interface IAsyncUnitOfWork<T> : IAsyncUnitOfWork, IRepositoryFactory<T>, IDisposable where T : DbContext
    { }
}
