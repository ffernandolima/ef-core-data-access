using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder.Interfaces
{
    public interface ISingleResultQuery<T> : IQueryBuilder<T, ISingleResultQuery<T>> where T : class
    {
        ISingleResultQuery<T, TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
    }

    public interface ISingleResultQuery<T, TResult> : IQueryBuilder<T, TResult, ISingleResultQuery<T, TResult>> where T : class
    { }
}
