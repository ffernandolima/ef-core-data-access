using EntityFrameworkCore.AutoHistory.Extensions;
using EntityFrameworkCore.Repository;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Factories;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace EntityFrameworkCore.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Private Fields

        private IDbContextTransaction _transaction;
        private ConcurrentDictionary<string, IRepository> _repositories;

        #endregion Private Fields

        #region Ctor

        /// <param name="dbContext">Injected</param>
        public UnitOfWork(DbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} cannot be null.");
            _repositories = new ConcurrentDictionary<string, IRepository>();
        }

        #endregion Ctor

        #region IRepositoryFactory Members

        /// <typeparam name="T">Custom repository interface</typeparam>
        public T CustomRepository<T>() where T : class
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException("Generic type should be an interface.");
            }

            static IRepository Factory(DbContext dbContext, Type type)
            {
                return (IRepository)AppDomain.CurrentDomain.GetAssemblies()
                                             .SelectMany(selector => selector.GetTypes())
                                             .Where(predicate => type.IsAssignableFrom(predicate) && !predicate.IsInterface && !predicate.IsAbstract)
                                             .Select(selector => Activator.CreateInstance(selector, dbContext))
                                             .SingleOrDefault();
            }

            return (T)GetRepository(typeof(T), Factory, "Custom");
        }

        public IRepository<T> Repository<T>() where T : class
        {
            static IRepository Factory(DbContext dbContext, Type type) => new Repository<T>(dbContext);

            return (IRepository<T>)GetRepository(typeof(T), Factory, "Generic");
        }

        #endregion IRepositoryFactory Members

        #region IUnitOfWork Members

        public DbContext DbContext { get; private set; }

        public TimeSpan? Timeout
        {
            get => DbContext.Database.GetCommandTimeout().HasValue ? new TimeSpan?(TimeSpan.FromSeconds(DbContext.Database.GetCommandTimeout().Value)) : null;
            set => DbContext.Database.SetCommandTimeout(value.HasValue ? new int?(Convert.ToInt32(value.Value.TotalSeconds)) : null);
        }

        #endregion IUnitOfWork Members

        #region ISyncUnitOfWork Members

        public bool HasTransaction() => _transaction != null;

        public bool HasChanges()
        {
            bool autoDetectChangesEnabled;

            if (!(autoDetectChangesEnabled = DbContext.ChangeTracker.AutoDetectChangesEnabled))
            {
                DbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            try
            {
                var hasChanges = DbContext.ChangeTracker.HasChanges();

                return hasChanges;
            }
            finally
            {
                DbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false)
        {
            if (!HasChanges())
            {
                return 0;
            }

            bool autoDetectChangesEnabled;

            if (!(autoDetectChangesEnabled = DbContext.ChangeTracker.AutoDetectChangesEnabled))
            {
                DbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            try
            {
                if (ensureAutoHistory)
                {
                    DbContext.EnsureAutoHistory();
                }

                return DbContext.SaveChanges(acceptAllChangesOnSuccess);
            }
            finally
            {
                DbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
        }

        public void DiscardChanges()
        {
            var dbEntityEntries = DbContext.ChangeTracker.Entries();

            foreach (var dbEntityEntry in dbEntityEntries)
            {
                dbEntityEntry.State = EntityState.Detached;
            }
        }

        public void BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Can't create more than one transaction.");
            }

            _transaction = DbContext.Database.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("Can't commit because the transaction is null.");
                }

                _transaction.Commit();
            }
            catch
            {
                Rollback();
                throw;
            }
            finally
            {
                DisposeTransaction();
            }
        }

        public void Rollback()
        {
            try
            {
                _transaction?.Rollback();
            }
            catch { }
            finally
            {
                DisposeTransaction();
            }
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var affectedRows = DbContext.Database.ExecuteSqlRaw(sql, parameters);

            return affectedRows;
        }

        public IList<T> FromSql<T>(string sql, params object[] parameters) where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var dbSet = DbContext.Set<T>();

            var entities = dbSet.FromSqlRaw(sql, parameters).ToList();

            return entities;
        }

        public void ChangeDatabase(string database)
        {
            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentException($"{nameof(database)} cannot be null or white-space.", nameof(database));
            }

            var dbConnection = DbContext.Database.GetDbConnection();

            if (dbConnection.State.HasFlag(ConnectionState.Open))
            {
                dbConnection.ChangeDatabase(database);
            }
            else
            {
                dbConnection.ConnectionString = Regex.Replace(dbConnection.ConnectionString.Replace(" ", string.Empty), @"(?<=[Dd]atabase=)\w+(?=;)", database, RegexOptions.Singleline);
            }

            var entityTypes = DbContext.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                if (entityType is IConventionEntityType conventionEntityType)
                {
                    conventionEntityType.SetSchema(database);
                }
            }
        }

        public void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback)
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

        public void TrackGraph<TState>(object rootEntity, TState state, Func<EntityEntryGraphNode<TState>, bool> callback)
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

        #endregion ISyncUnitOfWork Members

        #region IAsyncUnitOfWork Members

        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false, CancellationToken cancellationToken = default)
        {
            if (!HasChanges())
            {
                return Task.FromResult(0);
            }

            bool autoDetectChangesEnabled;

            if (!(autoDetectChangesEnabled = DbContext.ChangeTracker.AutoDetectChangesEnabled))
            {
                DbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            try
            {
                if (ensureAutoHistory)
                {
                    DbContext.EnsureAutoHistory();
                }

                return DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            finally
            {
                DbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
        }

        public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Can't create more than one transaction.");
            }

            _transaction = await DbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var affectedRows = DbContext.Database.ExecuteSqlRawAsync(sql, parameters ?? Enumerable.Empty<object>(), cancellationToken);

            return affectedRows;
        }

        #endregion IAsyncUnitOfWork Members

        #region Public Methods

        public static int SaveChanges(bool useTransaction = true, TimeSpan? timeout = null, bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false, params IUnitOfWork[] unitOfWorks)
        {
            if (!(unitOfWorks?.Any() ?? false))
            {
                return 0;
            }

            var count = 0;

            void SaveChangesInternal()
            {
                foreach (var unitOfWork in unitOfWorks)
                {
                    count += unitOfWork.SaveChanges(acceptAllChangesOnSuccess, ensureAutoHistory);
                }
            }

            if (useTransaction)
            {
                using (var transactionScope = TransactionScopeFactory.CreateTransactionScope(timeout: timeout ?? TransactionManager.MaximumTimeout))
                {
                    SaveChangesInternal();

                    transactionScope.Complete();
                }
            }
            else
            {
                SaveChangesInternal();
            }

            return count;
        }

        public static async Task<int> SaveChangesAsync(bool useTransaction = true, TimeSpan? timeout = null, bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false, CancellationToken cancellationToken = default, params IUnitOfWork[] unitOfWorks)
        {
            if (!(unitOfWorks?.Any() ?? false))
            {
                return await Task.FromResult(0);
            }

            var count = 0;

            async void SaveChangesAsyncInternal()
            {
                foreach (var unitOfWork in unitOfWorks)
                {
                    count += await unitOfWork.SaveChangesAsync(acceptAllChangesOnSuccess, ensureAutoHistory, cancellationToken);
                }
            }

            if (useTransaction)
            {
                using (var transactionScope = TransactionScopeFactory.CreateTransactionScope(timeout: timeout ?? TransactionManager.MaximumTimeout, transactionScopeAsyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
                {
                    SaveChangesAsyncInternal();

                    transactionScope.Complete();
                }
            }
            else
            {
                SaveChangesAsyncInternal();
            }

            return count;
        }

        #endregion Public Methods

        #region Private Methods

        private IRepository GetRepository(Type objectType, Func<DbContext, Type, IRepository> repositoryFactory, string prefix)
        {
            var typeName = $"{prefix}.{objectType.FullName}";

            if (!_repositories.TryGetValue(typeName, out var repository))
            {
                repository = repositoryFactory.Invoke(DbContext, objectType);

                _repositories[typeName] = repository;
            }

            return repository;
        }

        private void DisposeTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        #endregion Private Methods

        #region IDisposable Members

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeTransaction();

                    var connection = DbContext.Database.GetDbConnection();
                    if (connection != null && connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }

                    if (DbContext != null)
                    {
                        DbContext.Dispose();
                        DbContext = null;
                    }

                    if (_repositories != null)
                    {
                        foreach (var repository in _repositories.Values)
                        {
                            repository.Dispose();
                        }

                        _repositories.Clear();
                        _repositories = null;
                    }
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

    public class UnitOfWork<T> : UnitOfWork, IUnitOfWork<T> where T : DbContext
    {
        #region Ctor

        public UnitOfWork(T dbContext)
            : base(dbContext)
        { }

        #endregion Ctor
    }
}
