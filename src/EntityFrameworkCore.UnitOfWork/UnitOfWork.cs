using EntityFrameworkCore.Repository;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Factories;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        private DbContext _dbContext;
        private IDbContextTransaction _transaction;
        private ConcurrentDictionary<string, IRepository> _repositories;

        #endregion Private Fields

        #region Ctor

        /// <param name="dbContext">Injected</param>
        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} cannot be null.");
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

            IRepository factory(DbContext dbContext, Type type)
            {
                return (IRepository)AppDomain.CurrentDomain.GetAssemblies()
                                             .SelectMany(selector => selector.GetTypes())
                                             .Where(predicate => type.IsAssignableFrom(predicate) && !predicate.IsInterface && !predicate.IsAbstract)
                                             .Select(selector => Activator.CreateInstance(selector, dbContext))
                                             .SingleOrDefault();
            }

            return (T)GetRepository(typeof(T), factory, "Custom");
        }

        public IRepository<T> Repository<T>() where T : class
        {
            IRepository factory(DbContext dbContext, Type type) => new Repository<T>(dbContext);

            return (IRepository<T>)GetRepository(typeof(T), factory, "Generic");
        }

        #endregion IRepositoryFactory Members

        #region ISyncUnitOfWork Members

        public TimeSpan? Timeout
        {
            get { return _dbContext.Database.GetCommandTimeout().HasValue ? new TimeSpan?(TimeSpan.FromSeconds(_dbContext.Database.GetCommandTimeout().Value)) : null; }
            set { _dbContext.Database.SetCommandTimeout(value.HasValue ? new int?(Convert.ToInt32(value.Value.TotalSeconds)) : null); }
        }

        public bool HasTransaction() => _transaction != null;

        public bool HasChanges()
        {
            bool autoDetectChangesEnabled;

            if (!(autoDetectChangesEnabled = _dbContext.ChangeTracker.AutoDetectChangesEnabled))
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            try
            {
                var hasChanges = _dbContext.ChangeTracker.HasChanges();

                return hasChanges;
            }
            finally
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess = true)
        {
            if (!HasChanges())
            {
                return 0;
            }

            bool autoDetectChangesEnabled;

            if (!(autoDetectChangesEnabled = _dbContext.ChangeTracker.AutoDetectChangesEnabled))
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            try
            {
                return _dbContext.SaveChanges(acceptAllChangesOnSuccess);
            }
            finally
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
        }

        public void DiscardChanges()
        {
            var dbEntityEntries = _dbContext.ChangeTracker.Entries();

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

            _transaction = _dbContext.Database.BeginTransaction(isolationLevel);
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
                if (_transaction != null)
                {
                    _transaction.Rollback();
                }
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

            var affectedRows = _dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return affectedRows;
        }

        public IList<T> FromSql<T>(string sql, params object[] parameters) where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var dbSet = _dbContext.Set<T>();

            var entities = dbSet.FromSqlRaw(sql, parameters).ToList();

            return entities;
        }

        public void ChangeDatabase(string database)
        {
            var dbConnection = _dbContext.Database.GetDbConnection();

            if (dbConnection.State.HasFlag(ConnectionState.Open))
            {
                dbConnection.ChangeDatabase(database);
            }
            else
            {
                dbConnection.ConnectionString = Regex.Replace(dbConnection.ConnectionString.Replace(" ", string.Empty), @"(?<=[Dd]atabase=)\w+(?=;)", database, RegexOptions.Singleline);
            }

            var entityTypes = _dbContext.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                if (entityType is IConventionEntityType conventionEntityType)
                {
                    conventionEntityType.SetSchema(database);
                }
            }
        }

        #endregion ISyncUnitOfWork Members

        #region IAsyncUnitOfWork Members

        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        {
            if (!HasChanges())
            {
                return Task.FromResult(0);
            }

            bool autoDetectChangesEnabled;

            if (!(autoDetectChangesEnabled = _dbContext.ChangeTracker.AutoDetectChangesEnabled))
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            try
            {
                return _dbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            finally
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
        }

        public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Can't create more than one transaction.");
            }

            _transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"{nameof(sql)} cannot be null or white-space.", nameof(sql));
            }

            var affectedRows = _dbContext.Database.ExecuteSqlRawAsync(sql, parameters ?? Enumerable.Empty<object>(), cancellationToken);

            return affectedRows;
        }

        #endregion IAsyncUnitOfWork Members

        #region Public Methods

        public static int SaveChanges(bool useTransaction = true, TimeSpan? timeout = null, bool acceptAllChangesOnSuccess = true, params IUnitOfWork[] unitOfWorks)
        {
            if (!unitOfWorks?.Any() ?? false)
            {
                return 0;
            }

            var count = 0;

            void SaveChangesInternal()
            {
                foreach (var unitOfWork in unitOfWorks)
                {
                    count += unitOfWork.SaveChanges(acceptAllChangesOnSuccess);
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

        public static async Task<int> SaveChangesAsync(bool useTransaction = true, TimeSpan? timeout = null, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default, params IUnitOfWork[] unitOfWorks)
        {
            if (!unitOfWorks?.Any() ?? false)
            {
                return await Task.FromResult(0);
            }

            var count = 0;

            async void SaveChangesAsyncInternal()
            {
                foreach (var unitOfWork in unitOfWorks)
                {
                    count += await unitOfWork.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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

            if (!_repositories.TryGetValue(typeName, out IRepository repository))
            {
                repository = repositoryFactory.Invoke(_dbContext, objectType);

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

                    var connection = _dbContext.Database.GetDbConnection();
                    if (connection != null && connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }

                    if (_dbContext != null)
                    {
                        _dbContext.Dispose();
                        _dbContext = null;
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

        public UnitOfWork(DbContext dbContext)
            : base(dbContext)
        { }

        #endregion Ctor
    }
}
