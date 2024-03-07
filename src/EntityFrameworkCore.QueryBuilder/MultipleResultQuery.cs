using EntityFrameworkCore.QueryBuilder.Extensions;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder
{
    public class MultipleResultQuery<T> : Query<T, IMultipleResultQuery<T>>, IMultipleResultQuery<T> where T : class
    {
        public static IMultipleResultQuery<T> New() => new MultipleResultQuery<T>();

        protected override IMultipleResultQuery<T> BuilderInstance => this;

        #region Ctor

        internal MultipleResultQuery()
        { }

        #endregion Ctor

        #region IMultipleResultQuery<T> Members

        public IPaging Paging { get; internal set; } = new Paging();
        public ITopping Topping { get; internal set; } = new Topping();

        public IMultipleResultQuery<T> Page(int? pageIndex, int? pageSize)
        {
            if (Paging is Paging paging)
            {
                paging.PageIndex = pageIndex;
                paging.PageSize = pageSize;
            }

            return this;
        }

        public IMultipleResultQuery<T> Top(int? topRows)
        {
            if (Topping is Topping topping)
            {
                topping.TopRows = topRows;
            }

            return this;
        }

        public IMultipleResultQuery<T, TResult> Select<TResult>(Expression<Func<T, TResult>> selector) => this.ToQuery(selector);

        #endregion IMultipleResultQuery<T> Members
    }

    public class MultipleResultQuery<T, TResult> : Query<T, TResult, IMultipleResultQuery<T, TResult>>, IMultipleResultQuery<T, TResult> where T : class
    {
        public static IMultipleResultQuery<T, TResult> New() => new MultipleResultQuery<T, TResult>();

        protected override IMultipleResultQuery<T, TResult> BuilderInstance => this;

        #region Ctor

        internal MultipleResultQuery()
        { }

        #endregion Ctor

        #region IMultipleResultQuery<T, TResult> Members

        public IPaging Paging { get; internal set; } = new Paging();
        public ITopping Topping { get; internal set; } = new Topping();

        public IMultipleResultQuery<T, TResult> Page(int? pageIndex, int? pageSize)
        {
            if (Paging is Paging paging)
            {
                paging.PageIndex = pageIndex;
                paging.PageSize = pageSize;
            }

            return this;
        }

        public IMultipleResultQuery<T, TResult> Top(int? topRows)
        {
            if (Topping is Topping topping)
            {
                topping.TopRows = topRows;
            }

            return this;
        }

        #endregion IMultipleResultQuery<T, TResult> Members
    }
}
