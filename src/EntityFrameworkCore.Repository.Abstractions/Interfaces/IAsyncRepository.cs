using EntityFrameworkCore.QueryBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Repository.Interfaces
{
    public interface IAsyncRepository : IRepository, IDisposable
    { }

    public interface IAsyncRepository<T> : IAsyncRepository, IQueryFactory<T>, IDisposable where T : class
    {
        Task<IList<T>> SearchAsync(IQuery<T> query, CancellationToken cancellationToken = default);
        Task<IList<TResult>> SearchAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default);
        Task<T> SingleOrDefaultAsync(IQuery<T> query, CancellationToken cancellationToken = default);
        Task<TResult> SingleOrDefaultAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default);
        Task<T> FirstOrDefaultAsync(IQuery<T> query, CancellationToken cancellationToken = default);
        Task<TResult> FirstOrDefaultAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default);
        Task<T> LastOrDefaultAsync(IQuery<T> query, CancellationToken cancellationToken = default);
        Task<TResult> LastOrDefaultAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<long> LongCountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<decimal> AverageAsync(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> expression, CancellationToken cancellationToken = default);
        Task<int> RemoveAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IList<T>> FromSqlAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default);
        Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default);
        Task ReloadAsync(T entity, CancellationToken cancellationToken = default);
    }
}
