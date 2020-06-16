using EntityFrameworkCore.QueryBuilder.Interfaces;
using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder.Extensions
{
    internal static class QueryExtensions
    {
        public static IQuery<T, TResult> ToQuery<T, TResult>(this IQuery<T> sourceQuery, Expression<Func<T, TResult>> selector = null) where T : class
        {
            Query<T, TResult> destinationQuery = null;

            if (sourceQuery is ISingleResultQuery<T>)
            {
                destinationQuery = new SingleResultQuery<T, TResult>();
            }

            if (sourceQuery is IMultipleResultQuery<T> multipleResultQuery)
            {
                destinationQuery = new MultipleResultQuery<T, TResult>
                {
                    Paging = multipleResultQuery.Paging,
                    Topping = multipleResultQuery.Topping
                };
            }

            if (destinationQuery != null)
            {
                destinationQuery.IgnoreQueryFilters = sourceQuery.IgnoreQueryFilters;
                destinationQuery.QueryTrackingBehavior = sourceQuery.QueryTrackingBehavior;
                destinationQuery.Predicate = sourceQuery.Predicate;
                destinationQuery.Includes = sourceQuery.Includes;
                destinationQuery.Sortings = sourceQuery.Sortings;

                if (selector != null)
                {
                    destinationQuery.Selector = selector;
                }
            }

            return destinationQuery;
        }
    }
}
