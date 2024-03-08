﻿using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder.Interfaces
{
    public interface IMultipleResultQuery
    {
        IPaging Paging { get; }
        ITopping Topping { get; }
    }

    public interface IMultipleResultQuery<T> : IMultipleResultQuery, IQuery<T>, IQueryBuilder<T, IMultipleResultQuery<T>> where T : class
    {
        IMultipleResultQuery<T> Page(int? pageIndex, int? pageSize);
        IMultipleResultQuery<T> Top(int? topRows);
        IMultipleResultQuery<T, TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
    }

    public interface IMultipleResultQuery<T, TResult> : IMultipleResultQuery, IQuery<T, TResult>, IQueryBuilder<T, TResult, IMultipleResultQuery<T, TResult>> where T : class
    {
        IMultipleResultQuery<T, TResult> Page(int? pageIndex, int? pageSize);
        IMultipleResultQuery<T, TResult> Top(int? topRows);
    }
}
