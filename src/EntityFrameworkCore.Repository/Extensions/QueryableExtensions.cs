using EntityFrameworkCore.QueryBuilder;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using LinqKit;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.Repository.Extensions
{
    internal static class QueryableExtensions
    {
        public static IQueryable<T> Include<T>(this IQueryable<T> source, IList<Func<IQueryable<T>, IIncludableQueryable<T, object>>> includes) where T : class
        {
            if (!(includes?.Any() ?? false))
            {
                return source;
            }

            return includes.Aggregate(source, (queryable, include) => include == null ? queryable : include(queryable));
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) where T : class
        {
            if (predicate == null)
            {
                return source;
            }

            return source.AsExpandable().Where(predicate);
        }

        public static IQueryable<T> Top<T>(this IQueryable<T> source, ITopping topping) where T : class
        {
            if (!(topping?.TopRows > 0))
            {
                return source;
            }

            return source.Take(topping.TopRows.Value);
        }

        public static IQueryable<T> Page<T>(this IQueryable<T> source, IPaging paging) where T : class
        {
            if (!(paging?.PageSize > 0))
            {
                return source;
            }

            var skipCount = ((paging.PageIndex ?? 1) - 1) * paging.PageSize.Value;

            return skipCount < 0 ? source : source.Skip(skipCount).Take(paging.PageSize.Value);
        }

        public static IQueryable<T> Sort<T>(this IQueryable<T> source, IList<ISorting<T>> sortings) where T : class
        {
            if (!(sortings?.Any() ?? false))
            {
                return source;
            }

            var orderedQueryable = false;

            foreach (var sorting in sortings.Where(s => s != null))
            {
                if (sorting.SortDirection == SortDirection.Ascending)
                {
                    if (!orderedQueryable)
                    {
                        if (!string.IsNullOrWhiteSpace(sorting.FieldName))
                        {
                            source = source.OrderBy(sorting.FieldName, out var success);

                            if (success)
                            {
                                orderedQueryable = true;
                            }
                        }
                        else if (sorting.KeySelector != null)
                        {
                            source = source.OrderBy(sorting.KeySelector);

                            orderedQueryable = true;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(sorting.FieldName))
                        {
                            source = ((IOrderedQueryable<T>)source).ThenBy(sorting.FieldName, out _);
                        }
                        else if (sorting.KeySelector != null)
                        {
                            source = ((IOrderedQueryable<T>)source).ThenBy(sorting.KeySelector);
                        }
                    }
                }
                else if (sorting.SortDirection == SortDirection.Descending)
                {
                    if (!orderedQueryable)
                    {
                        if (!string.IsNullOrWhiteSpace(sorting.FieldName))
                        {
                            source = source.OrderByDescending(sorting.FieldName, out var success);

                            if (success)
                            {
                                orderedQueryable = true;
                            }
                        }
                        else if (sorting.KeySelector != null)
                        {
                            source = source.OrderByDescending(sorting.KeySelector);

                            orderedQueryable = true;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(sorting.FieldName))
                        {
                            source = ((IOrderedQueryable<T>)source).ThenByDescending(sorting.FieldName, out _);
                        }
                        else if (sorting.KeySelector != null)
                        {
                            source = ((IOrderedQueryable<T>)source).ThenByDescending(sorting.KeySelector);
                        }
                    }
                }
            }

            return source;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string fieldName, out bool success) where T : class
        {
            var expression = GenerateMethodCall(source, nameof(OrderBy), fieldName, out success);

            var queryable = (expression == null ? source : source.Provider.CreateQuery<T>(expression)) as IOrderedQueryable<T>;

            return queryable;
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string fieldName, out bool success) where T : class
        {
            var expression = GenerateMethodCall(source, nameof(OrderByDescending), fieldName, out success);

            var queryable = (expression == null ? source : source.Provider.CreateQuery<T>(expression)) as IOrderedQueryable<T>;

            return queryable;
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string fieldName, out bool success) where T : class
        {
            var expression = GenerateMethodCall(source, nameof(ThenBy), fieldName, out success);

            var queryable = expression == null ? source : source.Provider.CreateQuery<T>(expression) as IOrderedQueryable<T>;

            return queryable;
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string fieldName, out bool success) where T : class
        {
            var expression = GenerateMethodCall(source, nameof(ThenByDescending), fieldName, out success);

            var queryable = expression == null ? source : source.Provider.CreateQuery<T>(expression) as IOrderedQueryable<T>;

            return queryable;
        }

        private static MethodCallExpression GenerateMethodCall<T>(IQueryable<T> source, string methodName, string fieldName, out bool success) where T : class
        {
            try
            {
                var parameter = Expression.Parameter(typeof(T), "keySelector");

                var body = fieldName.Split('.').Aggregate<string, Expression>(parameter, Expression.PropertyOrField);

                var selector = Expression.Lambda(body, parameter);

                if (success = selector != null)
                {
                    var expression = Expression.Call(typeof(Queryable), methodName, new[] { typeof(T), body.Type }, source.Expression, selector);

                    return expression;
                }
            }
            catch { success = false; }

            return null;
        }
    }
}
