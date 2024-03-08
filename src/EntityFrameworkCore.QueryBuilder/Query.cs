using EntityFrameworkCore.QueryBuilder.Interfaces;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder
{
    public abstract class Query<T, TBuilder> : IQuery<T>, IQueryBuilder<T, TBuilder>
        where T : class
        where TBuilder : IQueryBuilder<T, TBuilder>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected abstract TBuilder BuilderInstance { get; }

        #region Ctor

        internal Query()
        { }

        #endregion Ctor

        #region IQuery<T> Members

        public bool? IgnoreQueryFilters { get; internal set; }
        public bool? IgnoreAutoIncludes { get; internal set; }
        public QueryTrackingBehavior? QueryTrackingBehavior { get; internal set; }
        public QuerySplittingBehavior? QuerySplittingBehavior { get; internal set; }
        public Expression<Func<T, bool>> Predicate { get; internal set; } = PredicateBuilder.New<T>(defaultExpression: true);
        public IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> Includes { get; internal set; } = new List<Func<IQueryable<T>, IIncludableQueryable<T, object>>>();
        public IList<ISorting<T>> Sortings { get; internal set; } = new List<ISorting<T>>();
        public Expression<Func<T, T>> Selector { get; internal set; }

        #endregion IQuery<T> Members

        #region IQueryBuilder<T, TBuilder> Members

        public TBuilder UseIgnoreQueryFilters(bool? ignoreQueryFilters)
        {
            IgnoreQueryFilters = ignoreQueryFilters;

            return BuilderInstance;
        }

        public TBuilder UseIgnoreAutoIncludes(bool? ignoreAutoIncludes)
        {
            IgnoreAutoIncludes = ignoreAutoIncludes;

            return BuilderInstance;
        }

        public TBuilder UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior)
        {
            QueryTrackingBehavior = queryTrackingBehavior;

            return BuilderInstance;
        }

        public TBuilder UseQuerySplittingBehavior(QuerySplittingBehavior? querySplittingBehavior)
        {
            QuerySplittingBehavior = querySplittingBehavior;

            return BuilderInstance;
        }

        public TBuilder AndFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate is not null)
            {
                Predicate = Predicate.And(predicate);
            }

            return BuilderInstance;
        }

        public TBuilder OrFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate is not null)
            {
                Predicate = Predicate.Or(predicate);
            }

            return BuilderInstance;
        }

        public TBuilder Include(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            if (includes is not null)
            {
                foreach (var include in includes)
                {
                    if (include is not null)
                    {
                        Includes.Add(include);
                    }
                }
            }

            return BuilderInstance;
        }

        public TBuilder OrderBy(Expression<Func<T, object>> keySelector)
        {
            if (keySelector is not null)
            {
                var sorting = new Sorting<T>
                {
                    KeySelector = keySelector,
                    SortDirection = SortDirection.Ascending
                };

                Sortings.Add(sorting);
            }

            return BuilderInstance;
        }

        public TBuilder ThenBy(Expression<Func<T, object>> keySelector) => OrderBy(keySelector);

        public TBuilder OrderBy(string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                var sorting = new Sorting<T>
                {
                    FieldName = fieldName,
                    SortDirection = SortDirection.Ascending
                };

                Sortings.Add(sorting);
            }

            return BuilderInstance;
        }

        public TBuilder ThenBy(string fieldName) => OrderBy(fieldName);

        public TBuilder OrderByDescending(Expression<Func<T, object>> keySelector)
        {
            if (keySelector is not null)
            {
                var sorting = new Sorting<T>
                {
                    KeySelector = keySelector,
                    SortDirection = SortDirection.Descending
                };

                Sortings.Add(sorting);
            }

            return BuilderInstance;
        }

        public TBuilder ThenByDescending(Expression<Func<T, object>> keySelector) => OrderByDescending(keySelector);

        public TBuilder OrderByDescending(string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                var sorting = new Sorting<T>
                {
                    FieldName = fieldName,
                    SortDirection = SortDirection.Descending
                };

                Sortings.Add(sorting);
            }

            return BuilderInstance;
        }

        public TBuilder ThenByDescending(string fieldName) => OrderByDescending(fieldName);

        public TBuilder Select(Expression<Func<T, T>> selector)
        {
            if (selector is not null)
            {
                Selector = selector;
            }

            return BuilderInstance;
        }

        #endregion IQueryBuilder<T, TBuilder> Members
    }

    public abstract class Query<T, TResult, TBuilder> : IQuery<T, TResult>, IQueryBuilder<T, TResult, TBuilder>
        where T : class
        where TBuilder : IQueryBuilder<T, TResult, TBuilder>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected abstract TBuilder BuilderInstance { get; }

        #region Ctor

        internal Query()
        { }

        #endregion Ctor

        #region IQuery<T, TResult> Members

        public bool? IgnoreQueryFilters { get; internal set; }
        public bool? IgnoreAutoIncludes { get; internal set; }
        public QueryTrackingBehavior? QueryTrackingBehavior { get; internal set; }
        public QuerySplittingBehavior? QuerySplittingBehavior { get; internal set; }
        public Expression<Func<T, bool>> Predicate { get; internal set; } = PredicateBuilder.New<T>(defaultExpression: true);
        public IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> Includes { get; internal set; } = new List<Func<IQueryable<T>, IIncludableQueryable<T, object>>>();
        public IList<ISorting<T>> Sortings { get; internal set; } = new List<ISorting<T>>();
        public Expression<Func<T, TResult>> Selector { get; internal set; }

        #endregion IQuery<T, TResult> Members

        #region IQueryBuilder<T, TResult, TBuilder> Members

        public TBuilder UseIgnoreQueryFilters(bool? ignoreQueryFilters)
        {
            IgnoreQueryFilters = ignoreQueryFilters;

            return BuilderInstance;
        }

        public TBuilder UseIgnoreAutoIncludes(bool? ignoreAutoIncludes)
        {
            IgnoreAutoIncludes = ignoreAutoIncludes;

            return BuilderInstance;
        }

        public TBuilder UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior)
        {
            QueryTrackingBehavior = queryTrackingBehavior;

            return BuilderInstance;
        }

        public TBuilder UseQuerySplittingBehavior(QuerySplittingBehavior? querySplittingBehavior)
        {
            QuerySplittingBehavior = querySplittingBehavior;

            return BuilderInstance;
        }

        public TBuilder AndFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate is not null)
            {
                Predicate = Predicate.And(predicate);
            }

            return BuilderInstance;
        }

        public TBuilder OrFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate is not null)
            {
                Predicate = Predicate.Or(predicate);
            }

            return BuilderInstance;
        }

        public TBuilder Include(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            if (includes is not null)
            {
                foreach (var include in includes)
                {
                    if (include is not null)
                    {
                        Includes.Add(include);
                    }
                }
            }

            return BuilderInstance;
        }

        public TBuilder OrderBy(Expression<Func<T, object>> keySelector)
        {
            if (keySelector is not null)
            {
                var sorting = new Sorting<T>
                {
                    KeySelector = keySelector,
                    SortDirection = SortDirection.Ascending
                };

                Sortings.Add(sorting);
            }

            return BuilderInstance;
        }

        public TBuilder ThenBy(Expression<Func<T, object>> keySelector) => OrderBy(keySelector);

        public TBuilder OrderBy(string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                var sorting = new Sorting<T>
                {
                    FieldName = fieldName,
                    SortDirection = SortDirection.Ascending
                };

                Sortings.Add(sorting);
            }

            return BuilderInstance;
        }

        public TBuilder ThenBy(string fieldName) => OrderBy(fieldName);

        public TBuilder OrderByDescending(Expression<Func<T, object>> keySelector)
        {
            if (keySelector is not null)
            {
                var sorting = new Sorting<T>
                {
                    KeySelector = keySelector,
                    SortDirection = SortDirection.Descending
                };

                Sortings.Add(sorting);
            }

            return BuilderInstance;
        }

        public TBuilder ThenByDescending(Expression<Func<T, object>> keySelector) => OrderByDescending(keySelector);

        public TBuilder OrderByDescending(string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                var sorting = new Sorting<T>
                {
                    FieldName = fieldName,
                    SortDirection = SortDirection.Descending
                };

                Sortings.Add(sorting);
            }

            return BuilderInstance;
        }

        public TBuilder ThenByDescending(string fieldName) => OrderByDescending(fieldName);

        public TBuilder Select(Expression<Func<T, TResult>> selector)
        {
            if (selector is not null)
            {
                Selector = selector;
            }

            return BuilderInstance;
        }

        #endregion IQueryBuilder<T, TResult, TBuilder> Members
    }
}
