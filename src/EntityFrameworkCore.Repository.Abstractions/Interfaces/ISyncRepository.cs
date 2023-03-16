using EntityFrameworkCore.QueryBuilder.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.Repository.Interfaces
{
    public interface ISyncRepository : IRepository, IDisposable
    { }

    public interface ISyncRepository<T> : ISyncRepository, IQueryFactory<T>, IDisposable where T : class
    {
        IList<T> Search(IQuery<T> query);
        IList<TResult> Search<TResult>(IQuery<T, TResult> query);
        T SingleOrDefault(IQuery<T> query);
        TResult SingleOrDefault<TResult>(IQuery<T, TResult> query);
        T FirstOrDefault(IQuery<T> query);
        TResult FirstOrDefault<TResult>(IQuery<T, TResult> query);
        T LastOrDefault(IQuery<T> query);
        TResult LastOrDefault<TResult>(IQuery<T, TResult> query);
        bool Any(Expression<Func<T, bool>> predicate = null);
        int Count(Expression<Func<T, bool>> predicate = null);
        long LongCount(Expression<Func<T, bool>> predicate = null);
        TResult Max<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null);
        TResult Min<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null);
        decimal Average(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null);
        decimal Sum(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null);
        T Attach(T entity);
        void AttachRange(IEnumerable<T> entities);
        T Add(T entity);
        void AddRange(IEnumerable<T> entities);
        T Update(T entity, params Expression<Func<T, object>>[] properties);
        int Update(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> expression);
        void UpdateRange(IEnumerable<T> entities, params Expression<Func<T, object>>[] properties);
        T Remove(T entity);
        int Remove(Expression<Func<T, bool>> predicate);
        void RemoveRange(IEnumerable<T> entities);
        int ExecuteSqlCommand(string sql, params object[] parameters);
        IList<T> FromSql(string sql, params object[] parameters);
        void ChangeTable(string table);
        void ChangeState(T entity, EntityState state);
        void Reload(T entity);
        void TrackGraph(T rootEntity, Action<EntityEntryGraphNode> callback);
        void TrackGraph<TState>(T rootEntity, TState state, Func<EntityEntryGraphNode<TState>, bool> callback);
        IQueryable<T> ToQueryable(IQuery<T> query);
        IQueryable<TResult> ToQueryable<TResult>(IQuery<T, TResult> query);
    }
}
