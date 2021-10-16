using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace EntityFrameworkCore.AutoHistory
{
    public class TrackedEntity
    {
        public EntityEntry EntityEntry { get; }
        public EntityState EntityState { get; }

        public TrackedEntity(EntityEntry entityEntry)
        {
            EntityEntry = entityEntry ?? throw new ArgumentNullException(nameof(entityEntry), $"{nameof(entityEntry)} cannot be null.");
            EntityState = entityEntry.State;
        }
    }
}
