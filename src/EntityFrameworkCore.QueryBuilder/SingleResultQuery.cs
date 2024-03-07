using EntityFrameworkCore.QueryBuilder.Extensions;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder
{
    public class SingleResultQuery<T> : Query<T, ISingleResultQuery<T>>, ISingleResultQuery<T> where T : class
    {
        public static ISingleResultQuery<T> New() => new SingleResultQuery<T>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override ISingleResultQuery<T> BuilderInstance => this;

        #region Ctor

        internal SingleResultQuery()
        { }

        #endregion Ctor

        #region ISingleResultQuery<T> Members

        public ISingleResultQuery<T, TResult> Select<TResult>(Expression<Func<T, TResult>> selector) => this.ToQuery(selector);

        #endregion ISingleResultQuery<T> Members
    }

    public class SingleResultQuery<T, TResult> : Query<T, TResult, ISingleResultQuery<T, TResult>>, ISingleResultQuery<T, TResult> where T : class
    {
        public static ISingleResultQuery<T, TResult> New() => new SingleResultQuery<T, TResult>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override ISingleResultQuery<T, TResult> BuilderInstance => this;

        #region Ctor

        internal SingleResultQuery()
        { }

        #endregion Ctor
    }
}
