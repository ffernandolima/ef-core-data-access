using EntityFrameworkCore.QueryBuilder.Interfaces;
using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder.Extensions
{
    internal static class QueryExtensions
    {
        public static ISingleResultQuery<T, TResult> ToQuery<T, TResult>(this ISingleResultQuery<T> sourceQuery, Expression<Func<T, TResult>> selector = null) where T : class
        {
            var destinationQuery = new SingleResultQuery<T, TResult>
            {
                IgnoreQueryFilters = sourceQuery.IgnoreQueryFilters,
                IgnoreAutoIncludes = sourceQuery.IgnoreAutoIncludes,
                QueryTrackingBehavior = sourceQuery.QueryTrackingBehavior,
                QuerySplittingBehavior = sourceQuery.QuerySplittingBehavior,
                Predicate = sourceQuery.Predicate,
                Includes = sourceQuery.Includes,
                Sortings = sourceQuery.Sortings,
                Selector = selector
            };

            return destinationQuery;
        }

        public static IMultipleResultQuery<T, TResult> ToQuery<T, TResult>(this IMultipleResultQuery<T> sourceQuery, Expression<Func<T, TResult>> selector = null) where T : class
        {
            var destinationQuery = new MultipleResultQuery<T, TResult>
            {
                IgnoreQueryFilters = sourceQuery.IgnoreQueryFilters,
                IgnoreAutoIncludes = sourceQuery.IgnoreAutoIncludes,
                QueryTrackingBehavior = sourceQuery.QueryTrackingBehavior,
                QuerySplittingBehavior = sourceQuery.QuerySplittingBehavior,
                Predicate = sourceQuery.Predicate,
                Includes = sourceQuery.Includes,
                Sortings = sourceQuery.Sortings,
                Paging = sourceQuery.Paging,
                Topping = sourceQuery.Topping,
                Selector = selector
            };

            return destinationQuery;
        }
    }
}
