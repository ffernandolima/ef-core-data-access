using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.AutoHistory.Serialization
{
    internal class EntityContractResolver : DefaultContractResolver
    {
        private readonly DbContext _dbContext;

        public EntityContractResolver(DbContext dbContext) => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        protected override IList<JsonProperty> CreateProperties(Type entityType, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(entityType, memberSerialization);

            var entityEntry = _dbContext.ChangeTracker.Entries().FirstOrDefault(entry => entityType == entry.Entity.GetType());

            if (entityEntry != null)
            {
                // Gets the navigations
                var navigations = entityEntry.Metadata.GetNavigations().Select(navigation => navigation.Name);

                // Excludes the navigation properties
                properties = properties.Where(property => !navigations.Contains(property.PropertyName)).ToArray();
            }

            return properties;
        }
    }
}
