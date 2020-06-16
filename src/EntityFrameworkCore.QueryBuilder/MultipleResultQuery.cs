using EntityFrameworkCore.QueryBuilder.Interfaces;

namespace EntityFrameworkCore.QueryBuilder
{
    public class MultipleResultQuery<T> : Query<T>, IMultipleResultQuery<T> where T : class
    {
        public static IMultipleResultQuery<T> New() => new MultipleResultQuery<T>();

        #region Ctor

        internal MultipleResultQuery()
        { }

        #endregion Ctor

        #region IMultipleResultQuery<T> Members

        public Paging Paging { get; internal set; } = new Paging();
        public Topping Topping { get; internal set; } = new Topping();

        public IMultipleResultQuery<T> Page(int? pageIndex, int? pageSize)
        {
            Paging.PageIndex = pageIndex;
            Paging.PageSize = pageSize;

            return this;
        }

        public IMultipleResultQuery<T> Top(int? topRows)
        {
            Topping.TopRows = topRows;

            return this;
        }

        #endregion IMultipleResultQuery<T> Members
    }

    public class MultipleResultQuery<T, TResult> : Query<T, TResult>, IMultipleResultQuery<T, TResult> where T : class
    {
        public static IMultipleResultQuery<T, TResult> New() => new MultipleResultQuery<T, TResult>();

        #region Ctor

        internal MultipleResultQuery()
        { }

        #endregion Ctor

        #region IMultipleResultQuery<T, TResult> Members

        public Paging Paging { get; internal set; } = new Paging();
        public Topping Topping { get; internal set; } = new Topping();

        public IMultipleResultQuery<T, TResult> Page(int? pageIndex, int? pageSize)
        {
            Paging.PageIndex = pageIndex;
            Paging.PageSize = pageSize;

            return this;
        }

        public IMultipleResultQuery<T, TResult> Top(int? topRows)
        {
            Topping.TopRows = topRows;

            return this;
        }

        #endregion IMultipleResultQuery<T, TResult> Members
    }
}
