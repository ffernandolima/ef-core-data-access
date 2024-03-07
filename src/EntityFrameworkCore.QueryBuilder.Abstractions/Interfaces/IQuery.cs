using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder.Interfaces
{
    public interface IQuery
    {
        bool? IgnoreQueryFilters { get; }
        bool? IgnoreAutoIncludes { get; }
        QueryTrackingBehavior? QueryTrackingBehavior { get; }
        QuerySplittingBehavior? QuerySplittingBehavior { get; }
    }

    public interface IQuery<T> : IQuery
        where T : class
    {
        Expression<Func<T, bool>> Predicate { get; }
        IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> Includes { get; }
        IList<ISorting<T>> Sortings { get; }
        Expression<Func<T, T>> Selector { get; }
    }

    public interface IQuery<T, TResult> : IQuery
        where T : class
    {
        Expression<Func<T, bool>> Predicate { get; }
        IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> Includes { get; }
        IList<ISorting<T>> Sortings { get; }
        Expression<Func<T, TResult>> Selector { get; }
    }

    public interface IQueryBuilder<T, TBuilder> : IQuery<T>
        where T : class
        where TBuilder : IQueryBuilder<T, TBuilder>
    {
        TBuilder UseIgnoreQueryFilters(bool? ignoreQueryFilters);
        TBuilder UseIgnoreAutoIncludes(bool? ignoreAutoIncludes);
        TBuilder UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior);
        TBuilder UseQuerySplittingBehavior(QuerySplittingBehavior? querySplittingBehavior);

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

    public interface IQueryBuilder<T, TResult, TBuilder> : IQuery<T, TResult>
        where T : class
        where TBuilder : IQueryBuilder<T, TResult, TBuilder>
    {
        TBuilder UseIgnoreQueryFilters(bool? ignoreQueryFilters);
        TBuilder UseIgnoreAutoIncludes(bool? ignoreAutoIncludes);
        TBuilder UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior);
        TBuilder UseQuerySplittingBehavior(QuerySplittingBehavior? querySplittingBehavior);

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
