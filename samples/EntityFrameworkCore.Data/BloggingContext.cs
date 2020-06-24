using EntityFrameworkCore.AutoHistory.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Data
{
    public class BloggingContext : DbContext
    {
        public BloggingContext(DbContextOptions<BloggingContext> options)
           : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder builder) 
        {
            builder.EnableAutoHistory();
            builder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly); 
        }
    }
}
