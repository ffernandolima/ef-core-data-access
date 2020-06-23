using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EntityFrameworkCore.AutoHistory.Extensions
{
    public static class DbContextExtensions
    {
        public static void EnsureAutoHistory(this DbContext dbContext) => EnsureAutoHistory(dbContext, () => new AutoHistory());

        public static void EnsureAutoHistory<TAutoHistory>(this DbContext dbContext, Func<TAutoHistory> historyFactory) where TAutoHistory : AutoHistory
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} cannot be null.");
            }

            if (historyFactory == null)
            {
                throw new ArgumentNullException(nameof(historyFactory), $"{nameof(historyFactory)} cannot be null.");
            }

            // Currently, it only supports Modified and Deleted entities
            // .ToArray() is needed here for excluding the AutoHistory model
            var entityEntries = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified || entry.State == EntityState.Deleted).ToArray();

            foreach (var entityEntry in entityEntries)
            {
                var autoHistory = entityEntry.AutoHistory(historyFactory);

                dbContext.Add(autoHistory);
            }
        }
    }
}
