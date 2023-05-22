using EntityFrameworkCore.QueryBuilder;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Extensions;
using EntityFrameworkCore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace EntityFrameworkCore.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        #region Protected Properties

        protected DbContext DbContext { get; }
        protected DbSet<T> DbSet { get; }

        #endregion

        #region Ctor

        public Repository(DbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} cannot be null.");
            DbSet = dbContext.Set<T>();
        }

        #endregion

        #region IQueryFactory<T> Members

        public virtual ISingleResultQuery<T> SingleResultQuery() => QueryBuilder.SingleResultQuery<T>.New();
        public virtual IMultipleResultQuery<T> MultipleResultQuery() => QueryBuilder.MultipleResultQuery<T>.New();

        public virtual ISingleResultQuery<T, TResult> SingleResultQuery<TResult>() => QueryBuilder.SingleResultQuery<T, TResult>.New();
        public virtual IMultipleResultQuery<T, TResult> MultipleResultQuery<TResult>() => QueryBuilder.MultipleResultQuery<T, TResult>.New();

        #endregion IQueryFactory<T> Members

        #region ISyncRepository<T> Members

        public virtual IList<T> Search(IQuery<T> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entities = queryable.ToList();

            return entities;
        }

        public virtual IList<TResult> Search<TResult>(IQuery<T, TResult> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            if (query.Selector == null)
            {
                throw new ArgumentNullException(nameof(query.Selector), $"{nameof(query.Selector)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entities = queryable.ToList();

            return entities;
        }

        public virtual T SingleOrDefault(IQuery<T> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.SingleOrDefault();

            return entity;
        }

        public virtual TResult SingleOrDefault<TResult>(IQuery<T, TResult> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            if (query.Selector == null)
            {
                throw new ArgumentNullException(nameof(query.Selector), $"{nameof(query.Selector)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.SingleOrDefault();

            return entity;
        }

        public virtual T FirstOrDefault(IQuery<T> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.FirstOrDefault();

            return entity;
        }

        public virtual TResult FirstOrDefault<TResult>(IQuery<T, TResult> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            if (query.Selector == null)
            {
                throw new ArgumentNullException(nameof(query.Selector), $"{nameof(query.Selector)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.FirstOrDefault();

            return entity;
        }

        public virtual T LastOrDefault(IQuery<T> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.LastOrDefault();

            return entity;
        }

        public virtual TResult LastOrDefault<TResult>(IQuery<T, TResult> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            if (query.Selector == null)
            {
                throw new ArgumentNullException(nameof(query.Selector), $"{nameof(query.Selector)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.LastOrDefault();

            return entity;
        }

        public virtual bool Any(Expression<Func<T, bool>> predicate = null)
        {
            var result = predicate == null ? DbSet.Any() : DbSet.Any(predicate);

            return result;
        }

        public virtual int Count(Expression<Func<T, bool>> predicate = null)
        {
            var result = predicate == null ? DbSet.Count() : DbSet.Count(predicate);

            return result;
        }

        public virtual long LongCount(Expression<Func<T, bool>> predicate = null)
        {
            var result = predicate == null ? DbSet.LongCount() : DbSet.LongCount(predicate);

            return result;
        }

        public virtual TResult Max<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector), $"{nameof(selector)} cannot be null.");
            }

            var result = predicate == null ? DbSet.Max(selector) : DbSet.Where(predicate).Max(selector);

            return result;
        }

        public virtual TResult Min<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector), $"{nameof(selector)} cannot be null.");
            }

            var result = predicate == null ? DbSet.Min(selector) : DbSet.Where(predicate).Min(selector);

            return result;
        }

        public virtual decimal Average(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector), $"{nameof(selector)} cannot be null.");
            }

            var result = predicate == null ? DbSet.Average(selector) : DbSet.Where(predicate).Average(selector);

            return result;
        }

        public virtual decimal Sum(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector), $"{nameof(selector)} cannot be null.");
            }

            var result = predicate == null ? DbSet.Sum(selector) : DbSet.Where(predicate).Sum(selector);

            return result;
        }

        public virtual T Attach(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{nameof(entity)} cannot be null.");
            }

            DbSet.Attach(entity);

            return entity;
        }

        public virtual void AttachRange(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), $"{nameof(entities)} cannot be null.");
            }

            if (!entities.Any())
            {
                return;
            }

            DbSet.AttachRange(entities);
        }

        public virtual T Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{nameof(entity)} cannot be null.");
            }

            DbSet.Add(entity);

            return entity;
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), $"{nameof(entities)} cannot be null.");
            }

            if (!entities.Any())
            {
                return;
            }

            DbSet.AddRange(entities);
        }

        public virtual T Update(T entity, params Expression<Func<T, object>>[] properties)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{nameof(entity)} cannot be null.");
            }

            if (properties?.Any() ?? false)
            {
                var entityEntry = DbContext.Entry(entity);

                foreach (var property in properties)
                {
                    PropertyEntry propertyEntry;

                    try
                    {
                        propertyEntry = entityEntry.Property(property);
                    }
                    catch { propertyEntry = null; }

                    if (propertyEntry != null)
                    {
                        propertyEntry.IsModified = true;
                    }
                    else
                    {
                        ReferenceEntry referenceEntry;

                        try
                        {
                            referenceEntry = entityEntry.Reference(property);
                        }
                        catch { referenceEntry = null; }

                        if (referenceEntry != null)
                        {
                            var referenceEntityEntry = referenceEntry.TargetEntry;

                            DbContext.Update(referenceEntityEntry.Entity);
                        }
                    }
                }
            }
            else
            {
                DbSet.Update(entity);
            }

            return entity;
        }

        public virtual int Update(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> expression)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), $"{nameof(predicate)} cannot be null.");
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression), $"{nameof(expression)} cannot be null.");
            }

            var result = DbSet.Where(predicate).Update(expression);

            return result;
        }

        public virtual void UpdateRange(IEnumerable<T> entities, params Expression<Func<T, object>>[] properties)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), $"{nameof(entities)} cannot be null.");
            }

            if (!entities.Any())
            {
                return;
            }

            foreach (var entity in entities)
            {
                Update(entity, properties);
            }
        }

        public virtual T Remove(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{nameof(entity)} cannot be null.");
            }

            DbSet.Remove(entity);

            return entity;
        }

        public virtual int Remove(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), $"{nameof(predicate)} cannot be null.");
            }

            var result = DbSet.Where(predicate).Delete();

            return result;
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), $"{nameof(entities)} cannot be null.");
            }

            if (!entities.Any())
            {
                return;
            }

            DbSet.RemoveRange(entities);
        }

        public virtual int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var affectedRows = DbContext.Database.ExecuteSqlRaw(sql, parameters);

            return affectedRows;
        }

        public virtual IList<T> FromSql(string sql, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var entities = DbSet.FromSqlRaw(sql, parameters).ToList();

            return entities;
        }

        public virtual void ChangeTable(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
            {
                throw new ArgumentException($"{nameof(table)} cannot be null or white-space.", nameof(table));
            }

            var entityType = DbContext.Model.FindEntityType(typeof(T));

            if (entityType is IConventionEntityType conventionEntityType)
            {
                conventionEntityType.SetTableName(table);
            }
        }

        public virtual void ChangeState(T entity, EntityState state)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{nameof(entity)} cannot be null.");
            }

            DbContext.Entry(entity).State = state;
        }

        public virtual void Reload(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{nameof(entity)} cannot be null.");
            }

            DbContext.Entry(entity).Reload();
        }


        public virtual void TrackGraph(T rootEntity, Action<EntityEntryGraphNode> callback)
        {
            if (rootEntity == null)
            {
                throw new ArgumentNullException(nameof(rootEntity), $"{nameof(rootEntity)} cannot be null.");
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback), $"{nameof(callback)} cannot be null.");
            }

            DbContext.ChangeTracker.TrackGraph(rootEntity, callback);
        }

        public virtual void TrackGraph<TState>(T rootEntity, TState state, Func<EntityEntryGraphNode<TState>, bool> callback)
        {
            if (rootEntity == null)
            {
                throw new ArgumentNullException(nameof(rootEntity), $"{nameof(rootEntity)} cannot be null.");
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback), $"{nameof(callback)} cannot be null.");
            }

            DbContext.ChangeTracker.TrackGraph<TState>(rootEntity, state, callback);
        }

        public virtual IQueryable<T> ToQueryable(IQuery<T> query)
        {
            IMultipleResultQuery<T> multipleResultQuery = null;

            if (query is IMultipleResultQuery<T>)
            {
                multipleResultQuery = (IMultipleResultQuery<T>)query;
            }

            var queryable = GetQueryable(query.QueryTrackingBehavior, query.QuerySplittingBehavior, query.IgnoreQueryFilters, query.IgnoreAutoIncludes);

            if (query.Includes.Any())
            {
                queryable = queryable.Include(query.Includes);
            }

            if (query.Predicate != null)
            {
                queryable = queryable.Filter(query.Predicate);
            }

            if (query.Sortings.Any())
            {
                queryable = queryable.Sort(query.Sortings);
            }

            if (multipleResultQuery != null && multipleResultQuery.Topping.IsEnabled)
            {
                queryable = queryable.Top(multipleResultQuery.Topping);
            }

            if (multipleResultQuery != null && multipleResultQuery.Paging.IsEnabled)
            {
                var countQueryable = GetQueryable(multipleResultQuery.QueryTrackingBehavior, query.QuerySplittingBehavior, multipleResultQuery.IgnoreQueryFilters, multipleResultQuery.IgnoreAutoIncludes);

                if (multipleResultQuery.Includes.Any())
                {
                    countQueryable = countQueryable.Include(multipleResultQuery.Includes);
                }

                if (multipleResultQuery.Predicate != null)
                {
                    countQueryable = countQueryable.Filter(multipleResultQuery.Predicate);
                }

                if (multipleResultQuery.Paging is Paging paging)
                {
                    paging.TotalCount = countQueryable.Count();
                }

                queryable = queryable.Page(multipleResultQuery.Paging);
            }

            if (query.Selector != null)
            {
                queryable = queryable.Select(query.Selector);
            }

            return queryable;
        }

        public virtual IQueryable<TResult> ToQueryable<TResult>(IQuery<T, TResult> query)
        {
            IMultipleResultQuery<T, TResult> multipleResultQuery = null;

            if (query is IMultipleResultQuery<T, TResult>)
            {
                multipleResultQuery = (IMultipleResultQuery<T, TResult>)query;
            }

            var queryable = GetQueryable(query.QueryTrackingBehavior, query.QuerySplittingBehavior, query.IgnoreQueryFilters, query.IgnoreAutoIncludes);

            if (query.Includes.Any())
            {
                queryable = queryable.Include(query.Includes);
            }

            if (query.Predicate != null)
            {
                queryable = queryable.Filter(query.Predicate);
            }

            if (query.Sortings.Any())
            {
                queryable = queryable.Sort(query.Sortings);
            }

            if (multipleResultQuery != null && multipleResultQuery.Topping.IsEnabled)
            {
                queryable = queryable.Top(multipleResultQuery.Topping);
            }

            if (multipleResultQuery != null && multipleResultQuery.Paging.IsEnabled)
            {
                var countQueryable = GetQueryable(multipleResultQuery.QueryTrackingBehavior, query.QuerySplittingBehavior, multipleResultQuery.IgnoreQueryFilters, multipleResultQuery.IgnoreAutoIncludes);

                if (multipleResultQuery.Includes.Any())
                {
                    countQueryable = countQueryable.Include(multipleResultQuery.Includes);
                }

                if (multipleResultQuery.Predicate != null)
                {
                    countQueryable = countQueryable.Filter(multipleResultQuery.Predicate);
                }

                if (multipleResultQuery.Paging is Paging paging)
                {
                    paging.TotalCount = countQueryable.Count();
                }

                queryable = queryable.Page(multipleResultQuery.Paging);
            }

            return queryable.Select(query.Selector);
        }

        #endregion ISyncRepository<T> Members

        #region IAsyncRepository<T> Members

        public virtual Task<IList<T>> SearchAsync(IQuery<T> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entities = queryable.ToListAsync(cancellationToken).Then<List<T>, IList<T>>(result => result, cancellationToken);

            return entities;
        }

        public virtual Task<IList<TResult>> SearchAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            if (query.Selector == null)
            {
                throw new ArgumentNullException(nameof(query.Selector), $"{nameof(query.Selector)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entities = queryable.ToListAsync(cancellationToken).Then<List<TResult>, IList<TResult>>(result => result, cancellationToken);

            return entities;
        }

        public virtual Task<T> SingleOrDefaultAsync(IQuery<T> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.SingleOrDefaultAsync(cancellationToken);

            return entity;
        }

        public virtual Task<TResult> SingleOrDefaultAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            if (query.Selector == null)
            {
                throw new ArgumentNullException(nameof(query.Selector), $"{nameof(query.Selector)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.SingleOrDefaultAsync(cancellationToken);

            return entity;
        }

        public virtual Task<T> FirstOrDefaultAsync(IQuery<T> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.FirstOrDefaultAsync(cancellationToken);

            return entity;
        }

        public virtual Task<TResult> FirstOrDefaultAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            if (query.Selector == null)
            {
                throw new ArgumentNullException(nameof(query.Selector), $"{nameof(query.Selector)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.FirstOrDefaultAsync(cancellationToken);

            return entity;
        }

        public virtual Task<T> LastOrDefaultAsync(IQuery<T> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.LastOrDefaultAsync(cancellationToken);

            return entity;
        }

        public virtual Task<TResult> LastOrDefaultAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), $"{nameof(query)} cannot be null.");
            }

            if (query.Selector == null)
            {
                throw new ArgumentNullException(nameof(query.Selector), $"{nameof(query.Selector)} cannot be null.");
            }

            var queryable = ToQueryable(query);

            var entity = queryable.LastOrDefaultAsync(cancellationToken);

            return entity;
        }

        public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var result = predicate == null ? DbSet.AnyAsync(cancellationToken) : DbSet.AnyAsync(predicate, cancellationToken);

            return result;
        }

        public virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var result = predicate == null ? DbSet.CountAsync(cancellationToken) : DbSet.CountAsync(predicate, cancellationToken);

            return result;
        }

        public virtual Task<long> LongCountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var result = predicate == null ? DbSet.LongCountAsync(cancellationToken) : DbSet.LongCountAsync(predicate, cancellationToken);

            return result;
        }

        public virtual Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector), $"{nameof(selector)} cannot be null.");
            }

            var result = predicate == null ? DbSet.MaxAsync(selector, cancellationToken) : DbSet.Where(predicate).MaxAsync(selector, cancellationToken);

            return result;
        }

        public virtual Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector), $"{nameof(selector)} cannot be null.");
            }

            var result = predicate == null ? DbSet.MinAsync(selector, cancellationToken) : DbSet.Where(predicate).MinAsync(selector, cancellationToken);

            return result;
        }

        public virtual Task<decimal> AverageAsync(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector), $"{nameof(selector)} cannot be null.");
            }

            var result = predicate == null ? DbSet.AverageAsync(selector, cancellationToken) : DbSet.Where(predicate).AverageAsync(selector, cancellationToken);

            return result;
        }

        public virtual Task<decimal> SumAsync(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector), $"{nameof(selector)} cannot be null.");
            }

            var result = predicate == null ? DbSet.SumAsync(selector, cancellationToken) : DbSet.Where(predicate).SumAsync(selector, cancellationToken);

            return result;
        }

        public virtual Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{nameof(entity)} cannot be null.");
            }

            var result = DbSet.AddAsync(entity, cancellationToken).AsTask().Then(result => result.Entity, cancellationToken);

            return result;
        }

        public virtual Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), $"{nameof(entities)} cannot be null.");
            }

            if (!entities.Any())
            {
                return Task.CompletedTask;
            }

            return DbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual Task<int> UpdateAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> expression, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), $"{nameof(predicate)} cannot be null.");
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression), $"{nameof(expression)} cannot be null.");
            }

            var result = DbSet.Where(predicate).UpdateAsync(expression, cancellationToken);

            return result;
        }

        public virtual Task<int> RemoveAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), $"{nameof(predicate)} cannot be null.");
            }

            var result = DbSet.Where(predicate).DeleteAsync();

            return result;
        }

        public virtual Task<IList<T>> FromSqlAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var entities = DbSet.FromSqlRaw(sql, parameters ?? Enumerable.Empty<object>()).ToListAsync(cancellationToken).Then<List<T>, IList<T>>(result => result, cancellationToken);

            return entities;
        }

        public virtual Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var affectedRows = DbContext.Database.ExecuteSqlRawAsync(sql, parameters ?? Enumerable.Empty<object>(), cancellationToken);

            return affectedRows;
        }

        public virtual Task ReloadAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{nameof(entity)} cannot be null.");
            }

            return DbContext.Entry(entity).ReloadAsync(cancellationToken);
        }

        #endregion IAsyncRepository<T> Members

        #region Private Methods

        private IQueryable<T> GetQueryable(QueryTrackingBehavior? queryTrackingBehavior = null, QuerySplittingBehavior? querySplittingBehavior = null, bool? ignoreQueryFilters = null, bool? ignoreAutoInclude = null)
        {
            IQueryable<T> queryable = DbSet;

            switch (queryTrackingBehavior)
            {
                case QueryTrackingBehavior.TrackAll:
                    {
                        queryable = queryable.AsTracking();
                    }
                    break;
                case QueryTrackingBehavior.NoTracking:
                    {
                        queryable = queryable.AsNoTracking();
                    }
                    break;
                default:
                    break;
            }

            switch (querySplittingBehavior)
            {
                case QuerySplittingBehavior.SingleQuery:
                    {
                        queryable = queryable.AsSingleQuery();
                    }
                    break;
                case QuerySplittingBehavior.SplitQuery:
                    {
                        queryable = queryable.AsSplitQuery();
                    }
                    break;
                default:
                    break;
            }

            if (ignoreQueryFilters ?? false)
            {
                queryable = queryable.IgnoreQueryFilters();
            }

            if (ignoreAutoInclude ?? false)
            {
                queryable = queryable.IgnoreAutoIncludes();
            }

            return queryable;
        }

        #endregion

        #region IDisposable Members

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}
