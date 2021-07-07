using EntityFrameworkCore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Repository.Extensions
{
    public static class RepositoryExtensions
    {
        public static void RemoveTracking<T>(this IRepository<T> repository, T rootEntity) where T : class
        {
            repository.TrackGraph(rootEntity, e =>
            {
                foreach (var dbEntityEntry in e.Entry.Context.ChangeTracker.Entries())
                {
                    if (dbEntityEntry.Entity != null)
                    {
                        if (dbEntityEntry.State == EntityState.Unchanged)
                        {
                            dbEntityEntry.State = EntityState.Detached;
                        }
                    }
                }
            });
        }
    }
}
