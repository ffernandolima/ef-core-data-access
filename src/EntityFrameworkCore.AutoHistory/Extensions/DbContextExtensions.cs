using EntityFrameworkCore.AutoHistory.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EntityFrameworkCore.AutoHistory.Extensions
{
    public static class DbContextExtensions
    {
        public static void EnsureAutoHistory(this DbContext dbContext)
            => EnsureAutoHistory(dbContext, () => new AutoHistory());

        public static void EnsureAutoHistory(this DbContext dbContext, params EntityState[] entityStates)
           => EnsureAutoHistory(dbContext, () => new AutoHistory(), dbContext?.DetectChanges(entityStates));

        public static void EnsureAutoHistory(this DbContext dbContext, params TrackedEntity[] trackedEntities)
            => EnsureAutoHistory(dbContext, () => new AutoHistory(), trackedEntities);

        public static void EnsureAutoHistory<TAutoHistory>(this DbContext dbContext, Func<TAutoHistory> historyFactory) where TAutoHistory : AutoHistory
            => EnsureAutoHistory(dbContext, historyFactory, new[] { EntityState.Modified, EntityState.Deleted });

        public static void EnsureAutoHistory<TAutoHistory>(this DbContext dbContext, Func<TAutoHistory> historyFactory, params EntityState[] entityStates) where TAutoHistory : AutoHistory
            => EnsureAutoHistory(dbContext, historyFactory, dbContext?.DetectChanges(entityStates));

        public static void EnsureAutoHistory<TAutoHistory>(this DbContext dbContext, Func<TAutoHistory> historyFactory, params TrackedEntity[] trackedEntities) where TAutoHistory : AutoHistory
        {
            if (dbContext is null)
            {
                throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} cannot be null.");
            }

            if (historyFactory is null)
            {
                throw new ArgumentNullException(nameof(historyFactory), $"{nameof(historyFactory)} cannot be null.");
            }

            if (trackedEntities is null)
            {
                throw new ArgumentNullException(nameof(trackedEntities), $"{nameof(trackedEntities)} cannot be null.");
            }

            foreach (var trackedEntity in trackedEntities.Where(entity => entity is not null))
            {
                // The EntityState property is not affected by SaveChanges
                var entityEntry = trackedEntity.EntityEntry;
                var entityState = trackedEntity.EntityState;

                var autoHistory = entityEntry.AutoHistory(entityState, historyFactory);

                if (autoHistory is not null)
                {
                    dbContext.Add(autoHistory);
                }
            }
        }

        public static TrackedEntity[] DetectChanges(this DbContext dbContext, params EntityState[] entityStates)
        {
            if (dbContext is null)
            {
                throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} cannot be null.");
            }

            if (entityStates is null)
            {
                throw new ArgumentNullException(nameof(entityStates), $"{nameof(entityStates)} cannot be null.");
            }

            // .ToArray() is needed here for excluding the AutoHistory model
            var trackedEntities = dbContext.ChangeTracker.Entries()
                                                         .Where(entry => entry.Metadata.ClrType.GetCustomAttributes(typeof(ExcludeFromHistoryAttribute), inherit: true).Length == 0)
                                                         .Where(entry => entityStates.Contains(entry.State))
                                                         .Select(entry => new TrackedEntity(entry))
                                                         .ToArray();
            return trackedEntities;
        }
    }
}