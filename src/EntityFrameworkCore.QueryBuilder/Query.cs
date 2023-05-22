using EntityFrameworkCore.QueryBuilder.Extensions;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder
{
    public abstract class Query<T> : IQuery<T> where T : class
    {
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

        public IQuery<T> UseIgnoreQueryFilters(bool? ignoreQueryFilters)
        {
            IgnoreQueryFilters = ignoreQueryFilters;

            return this;
        }

        public IQuery<T> UseIgnoreAutoIncludes(bool? ignoreAutoIncludes)
        {
            IgnoreAutoIncludes = ignoreAutoIncludes;

            return this;
        }

        public IQuery<T> UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior)
        {
            QueryTrackingBehavior = queryTrackingBehavior;

            return this;
        }

        public IQuery<T> UseQuerySplittingBehavior(QuerySplittingBehavior? querySplittingBehavior)
        {
            QuerySplittingBehavior = querySplittingBehavior;

            return this;
        }

        public IQuery<T> AndFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                Predicate = Predicate.And(predicate);
            }

            return this;
        }

        public IQuery<T> OrFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                Predicate = Predicate.Or(predicate);
            }

            return this;
        }

        public IQuery<T> Include(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    if (include != null)
                    {
                        Includes.Add(include);
                    }
                }
            }

            return this;
        }

        public IQuery<T> OrderBy(Expression<Func<T, object>> keySelector)
        {
            if (keySelector != null)
            {
                var sorting = new Sorting<T>
                {
                    KeySelector = keySelector,
                    SortDirection = SortDirection.Ascending
                };

                Sortings.Add(sorting);
            }

            return this;
        }

        public IQuery<T> ThenBy(Expression<Func<T, object>> keySelector) => OrderBy(keySelector);

        public IQuery<T> OrderBy(string fieldName)
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

            return this;
        }

        public IQuery<T> ThenBy(string fieldName) => OrderBy(fieldName);

        public IQuery<T> OrderByDescending(Expression<Func<T, object>> keySelector)
        {
            if (keySelector != null)
            {
                var sorting = new Sorting<T>
                {
                    KeySelector = keySelector,
                    SortDirection = SortDirection.Descending
                };

                Sortings.Add(sorting);
            }

            return this;
        }

        public IQuery<T> ThenByDescending(Expression<Func<T, object>> keySelector) => OrderByDescending(keySelector);

        public IQuery<T> OrderByDescending(string fieldName)
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

            return this;
        }

        public IQuery<T> ThenByDescending(string fieldName) => OrderByDescending(fieldName);

        public IQuery<T> Select(Expression<Func<T, T>> selector)
        {
            if (selector != null)
            {
                Selector = selector;
            }

            return this;
        }

        public IQuery<T, TResult> Select<TResult>(Expression<Func<T, TResult>> selector) => this.ToQuery(selector);

        #endregion IQuery<T> Members
    }

    public abstract class Query<T, TResult> : IQuery<T, TResult> where T : class
    {
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

        public IQuery<T, TResult> UseIgnoreQueryFilters(bool? ignoreQueryFilters)
        {
            IgnoreQueryFilters = ignoreQueryFilters;

            return this;
        }

        public IQuery<T, TResult> UseIgnoreAutoIncludes(bool? ignoreAutoIncludes)
        {
            IgnoreAutoIncludes = ignoreAutoIncludes;

            return this;
        }

        public IQuery<T, TResult> UseQueryTrackingBehavior(QueryTrackingBehavior? queryTrackingBehavior)
        {
            QueryTrackingBehavior = queryTrackingBehavior;

            return this;
        }

        public IQuery<T, TResult> UseQuerySplittingBehavior(QuerySplittingBehavior? querySplittingBehavior)
        {
            QuerySplittingBehavior = querySplittingBehavior;

            return this;
        }

        public IQuery<T, TResult> AndFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                Predicate = Predicate.And(predicate);
            }

            return this;
        }

        public IQuery<T, TResult> OrFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                Predicate = Predicate.Or(predicate);
            }

            return this;
        }

        public IQuery<T, TResult> Include(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    if (include != null)
                    {
                        Includes.Add(include);
                    }
                }
            }

            return this;
        }

        public IQuery<T, TResult> OrderBy(Expression<Func<T, object>> keySelector)
        {
            if (keySelector != null)
            {
                var sorting = new Sorting<T>
                {
                    KeySelector = keySelector,
                    SortDirection = SortDirection.Ascending
                };

                Sortings.Add(sorting);
            }

            return this;
        }

        public IQuery<T, TResult> ThenBy(Expression<Func<T, object>> keySelector) => OrderBy(keySelector);

        public IQuery<T, TResult> OrderBy(string fieldName)
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

            return this;
        }

        public IQuery<T, TResult> ThenBy(string fieldName) => OrderBy(fieldName);

        public IQuery<T, TResult> OrderByDescending(Expression<Func<T, object>> keySelector)
        {
            if (keySelector != null)
            {
                var sorting = new Sorting<T>
                {
                    KeySelector = keySelector,
                    SortDirection = SortDirection.Descending
                };

                Sortings.Add(sorting);
            }

            return this;
        }

        public IQuery<T, TResult> ThenByDescending(Expression<Func<T, object>> keySelector) => OrderByDescending(keySelector);

        public IQuery<T, TResult> OrderByDescending(string fieldName)
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

            return this;
        }

        public IQuery<T, TResult> ThenByDescending(string fieldName) => OrderByDescending(fieldName);

        public IQuery<T, TResult> Select(Expression<Func<T, TResult>> selector)
        {
            if (selector != null)
            {
                Selector = selector;
            }

            return this;
        }

        #endregion IQuery<T, TResult> Members
    }
}
