using EntityFrameworkCore.QueryBuilder.Interfaces;

namespace EntityFrameworkCore.QueryBuilder
{
    public class SingleResultQuery<T> : Query<T>, ISingleResultQuery<T> where T : class
    {
        public static ISingleResultQuery<T> New() => new SingleResultQuery<T>();

        #region Ctor

        internal SingleResultQuery()
        { }

        #endregion Ctor
    }

    public class SingleResultQuery<T, TResult> : Query<T, TResult>, ISingleResultQuery<T, TResult> where T : class
    {
        public static ISingleResultQuery<T, TResult> New() => new SingleResultQuery<T, TResult>();

        #region Ctor

        internal SingleResultQuery()
        { }

        #endregion Ctor
    }
}
