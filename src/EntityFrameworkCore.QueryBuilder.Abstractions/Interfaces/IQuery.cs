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

    public interface IQuery<T> : IQuery where T : class
    {
        Expression<Func<T, bool>> Predicate { get; }
        IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> Includes { get; }
        IList<ISorting<T>> Sortings { get; }
        Expression<Func<T, T>> Selector { get; }

        IQuery<T> UseIgnoreQueryFilters(bool? ignoreQueryFilters);
        IQuery<T> UseIgnoreAutoIncludes(bool? ignoreAutoIncludes);
        IQuery<T> UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior);
        IQuery<T> UseQuerySplittingBehavior(QuerySplittingBehavior? querySplittingBehavior);

        IQuery<T> AndFilter(Expression<Func<T, bool>> predicate);
        IQuery<T> OrFilter(Expression<Func<T, bool>> predicate);

        IQuery<T> Include(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        IQuery<T> OrderBy(Expression<Func<T, object>> keySelector);
        IQuery<T> ThenBy(Expression<Func<T, object>> keySelector);
        IQuery<T> OrderBy(string fieldName);
        IQuery<T> ThenBy(string fieldName);

        IQuery<T> OrderByDescending(Expression<Func<T, object>> keySelector);
        IQuery<T> ThenByDescending(Expression<Func<T, object>> keySelector);
        IQuery<T> OrderByDescending(string fieldName);
        IQuery<T> ThenByDescending(string fieldName);

        IQuery<T> Select(Expression<Func<T, T>> selector);
        IQuery<T, TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
    }

    public interface IQuery<T, TResult> : IQuery where T : class
    {
        Expression<Func<T, bool>> Predicate { get; }
        IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> Includes { get; }
        Expression<Func<T, TResult>> Selector { get; }
        IList<ISorting<T>> Sortings { get; }

        IQuery<T, TResult> UseIgnoreQueryFilters(bool? ignoreQueryFilters);
        IQuery<T, TResult> UseIgnoreAutoIncludes(bool? ignoreAutoIncludes);
        IQuery<T, TResult> UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior);
        IQuery<T, TResult> UseQuerySplittingBehavior(QuerySplittingBehavior? querySplittingBehavior);

        IQuery<T, TResult> AndFilter(Expression<Func<T, bool>> predicate);
        IQuery<T, TResult> OrFilter(Expression<Func<T, bool>> predicate);

        IQuery<T, TResult> Include(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        IQuery<T, TResult> OrderBy(Expression<Func<T, object>> keySelector);
        IQuery<T, TResult> ThenBy(Expression<Func<T, object>> keySelector);
        IQuery<T, TResult> OrderBy(string fieldName);
        IQuery<T, TResult> ThenBy(string fieldName);

        IQuery<T, TResult> OrderByDescending(Expression<Func<T, object>> keySelector);
        IQuery<T, TResult> ThenByDescending(Expression<Func<T, object>> keySelector);
        IQuery<T, TResult> OrderByDescending(string fieldName);
        IQuery<T, TResult> ThenByDescending(string fieldName);

        IQuery<T, TResult> Select(Expression<Func<T, TResult>> selector);
    }
}
