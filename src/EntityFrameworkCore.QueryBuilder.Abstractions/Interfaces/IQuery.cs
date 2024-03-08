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
        QueryTrackingBehavior? QueryTrackingBehavior { get; }
    }

    public interface IQuery<T> : IQuery where T : class
    {
        Expression<Func<T, bool>> Predicate { get; }
        IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> Includes { get; }
        IList<ISorting<T>> Sortings { get; }
        Expression<Func<T, T>> Selector { get; }
    }

    public interface IQuery<T, TResult> : IQuery where T : class
    {
        Expression<Func<T, bool>> Predicate { get; }
        IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> Includes { get; }
        Expression<Func<T, TResult>> Selector { get; }
        IList<ISorting<T>> Sortings { get; }
        Expression<Func<T, TResult>> Selector { get; }
    }
}
