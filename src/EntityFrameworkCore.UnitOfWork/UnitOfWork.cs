using EntityFrameworkCore.AutoHistory.Extensions;
using EntityFrameworkCore.Repository;
using EntityFrameworkCore.Repository.Extensions;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Factories;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace EntityFrameworkCore.UnitOfWork
{
    using System.Data;

    public class UnitOfWork : IUnitOfWork
    {
        #region Private Fields

        private IDbContextTransaction _transaction;
        private readonly ConcurrentDictionary<string, IRepository> _repositories;

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

            return DbContext.GetInfrastructure()?.GetService<T>() ?? (T)GetRepository(typeof(T), Factory, "Custom");
        }

        public IRepository<T> Repository<T>() where T : class
        {
            static IRepository Factory(DbContext dbContext, Type type) => new Repository<T>(dbContext);

            return DbContext.GetInfrastructure()?.GetService<IRepository<T>>() ?? (IRepository<T>)GetRepository(typeof(T), Factory, "Generic");
        }

        #endregion IRepositoryFactory Members

        #region IUnitOfWork Members

        public DbContext DbContext { get; }

        public TimeSpan? Timeout
        {
            get
            {
                var commandTimeout = DbContext.Database.GetCommandTimeout();

                return commandTimeout.HasValue ? new TimeSpan?(TimeSpan.FromSeconds(commandTimeout.Value)) : null;
            }
            set
            {
                var commandTimeout = value.HasValue ? new int?(Convert.ToInt32(value.Value.TotalSeconds)) : null;

                DbContext.Database.SetCommandTimeout(commandTimeout);
            }
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

        public void UseTransaction(DbTransaction transaction, Guid? transactionId = null)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), $"{nameof(transaction)} cannot be null.");
            }

            if (_transaction != null)
            {
                throw new InvalidOperationException("There's already an active transaction.");
            }

            _transaction = !transactionId.HasValue ? DbContext.Database.UseTransaction(transaction) : DbContext.Database.UseTransaction(transaction, transactionId.Value);
        }

        public void EnlistTransaction(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), $"{nameof(transaction)} cannot be null.");
            }

            DbContext.Database.EnlistTransaction(transaction);
        }

        public Transaction GetEnlistedTransaction() => DbContext.Database.GetEnlistedTransaction();

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("There's already an active transaction.");
            }

            _transaction = DbContext.Database.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("There's no active transaction.");
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
            catch
            {
                // ignored
            }
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

        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false, CancellationToken cancellationToken = default)
        {
            if (!HasChanges())
            {
                return await Task.FromResult(0).ConfigureAwait(continueOnCapturedContext: false);
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

                return await DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            finally
            {
                DbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
        }

        public async Task UseTransactionAsync(DbTransaction transaction, Guid? transactionId = null, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), $"{nameof(transaction)} cannot be null.");
            }

            if (_transaction != null)
            {
                throw new InvalidOperationException("There's already an active transaction.");
            }

            _transaction = !transactionId.HasValue
                ? await DbContext.Database.UseTransactionAsync(transaction, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)
                : await DbContext.Database.UseTransactionAsync(transaction, transactionId.Value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("There's already an active transaction.");
            }

            _transaction = await DbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("There's no active transaction.");
                }

                await _transaction.CommitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch
            {
                using (var source = new CancellationTokenSource())
                {
                    await RollbackAsync(source.Token).ConfigureAwait(continueOnCapturedContext: false);
                }

                throw;
            }
            finally
            {
                await DisposeTransactionAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                await DisposeTransactionAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public virtual Task<IList<T>> FromSqlAsync<T>(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var dbSet = DbContext.Set<T>();

            var entities = dbSet.FromSqlRaw(sql, parameters ?? Enumerable.Empty<object>()).ToListAsync(cancellationToken).Then<List<T>, IList<T>>(result => result, cancellationToken);

            return entities;
        }

        public async Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var affectedRows = await DbContext.Database.ExecuteSqlRawAsync(sql, parameters ?? Enumerable.Empty<object>(), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

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
                using var transactionScope = TransactionScopeFactory.CreateTransactionScope(timeout: timeout ?? TransactionManager.MaximumTimeout);

                SaveChangesInternal();

                transactionScope.Complete();
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
                return await Task.FromResult(0).ConfigureAwait(continueOnCapturedContext: false);
            }

            var count = 0;

            async Task SaveChangesAsyncInternal()
            {
                foreach (var unitOfWork in unitOfWorks)
                {
                    count += await unitOfWork.SaveChangesAsync(acceptAllChangesOnSuccess, ensureAutoHistory, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }
            }

            if (useTransaction)
            {
                using var transactionScope = TransactionScopeFactory.CreateTransactionScope(timeout: timeout ?? TransactionManager.MaximumTimeout, transactionScopeAsyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);

                await SaveChangesAsyncInternal().ConfigureAwait(continueOnCapturedContext: false);

                transactionScope.Complete();
            }
            else
            {
                await SaveChangesAsyncInternal().ConfigureAwait(continueOnCapturedContext: false);
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

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
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

                    if (DbContext.Database.IsRelational())
                    {
                        var connection = DbContext.Database.GetDbConnection();
                        if (connection != null && connection.State != ConnectionState.Closed)
                        {
                            connection.Close();
                        }

                        DbContext.Dispose();
                    }

                    foreach (var repository in _repositories.Values)
                    {
                        repository.Dispose();
                    }

                    _repositories.Clear();
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

    public class PooledUnitOfWork<T> : UnitOfWork<T>, IPooledUnitOfWork<T> where T : DbContext
    {
        #region Ctor

        public PooledUnitOfWork(IDbContextFactory<T> dbContextFactory)
            : base(CreateDbContext(dbContextFactory))
        {
            DbContextFactory = dbContextFactory;
        }

        #endregion Ctor

        #region IPooledUnitOfWork Members

        public IDbContextFactory<T> DbContextFactory { get; }

        #endregion IPooledUnitOfWork Members

        #region Private Methods

        private static T CreateDbContext(IDbContextFactory<T> dbContextFactory)
        {
            if (dbContextFactory == null)
            {
                throw new ArgumentNullException(nameof(dbContextFactory), $"{nameof(dbContextFactory)} cannot be null.");
            }

            var dbContext = dbContextFactory.CreateDbContext();

            return dbContext;
        }

        #endregion Private Methods
    }
}
