using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder.Interfaces
{
    public interface IQueryBuilder<T, TBuilder>
        where T : class
        where TBuilder : IQueryBuilder<T, TBuilder>
    {
        TBuilder UseIgnoreQueryFilters(bool? ignoreQueryFilters);
        TBuilder UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior);

        TBuilder AndFilter(Expression<Func<T, bool>> predicate);
        TBuilder OrFilter(Expression<Func<T, bool>> predicate);

        TBuilder Include(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        TBuilder OrderBy(Expression<Func<T, object>> keySelector);
        TBuilder ThenBy(Expression<Func<T, object>> keySelector);
        TBuilder OrderBy(string fieldName);
        TBuilder ThenBy(string fieldName);

        TBuilder OrderByDescending(Expression<Func<T, object>> keySelector);
        TBuilder ThenByDescending(Expression<Func<T, object>> keySelector);
        TBuilder OrderByDescending(string fieldName);
        TBuilder ThenByDescending(string fieldName);

        TBuilder Select(Expression<Func<T, T>> selector);
    }

    public interface IQueryBuilder<T, TResult, TBuilder>
        where T : class
        where TBuilder : IQueryBuilder<T, TResult, TBuilder>
    {
        TBuilder UseIgnoreQueryFilters(bool? ignoreQueryFilters);
        TBuilder UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior);

        TBuilder AndFilter(Expression<Func<T, bool>> predicate);
        TBuilder OrFilter(Expression<Func<T, bool>> predicate);

        TBuilder Include(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        TBuilder OrderBy(Expression<Func<T, object>> keySelector);
        TBuilder ThenBy(Expression<Func<T, object>> keySelector);
        TBuilder OrderBy(string fieldName);
        TBuilder ThenBy(string fieldName);

        TBuilder OrderByDescending(Expression<Func<T, object>> keySelector);
        TBuilder ThenByDescending(Expression<Func<T, object>> keySelector);
        TBuilder OrderByDescending(string fieldName);
        TBuilder ThenByDescending(string fieldName);

        TBuilder Select(Expression<Func<T, TResult>> selector);
    }
}
