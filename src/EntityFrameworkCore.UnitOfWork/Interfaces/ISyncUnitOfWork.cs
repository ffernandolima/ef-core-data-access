using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data;

namespace EntityFrameworkCore.UnitOfWork.Interfaces
{
    public interface ISyncUnitOfWork : IRepositoryFactory, IDisposable
    {
        bool HasTransaction();
        bool HasChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false);
        void DiscardChanges();
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void Commit();
        void Rollback();
        int ExecuteSqlCommand(string sql, params object[] parameters);
        IList<T> FromSql<T>(string sql, params object[] parameters) where T : class;
        void ChangeDatabase(string database);
        void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback);
        void TrackGraph<TState>(object rootEntity, TState state, Func<EntityEntryGraphNode, TState, bool> callback);
    }

    public interface ISyncUnitOfWork<T> : ISyncUnitOfWork, IRepositoryFactory<T>, IDisposable where T : DbContext
    { }
}
