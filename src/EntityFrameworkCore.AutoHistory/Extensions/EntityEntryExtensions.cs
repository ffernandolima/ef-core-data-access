using EntityFrameworkCore.AutoHistory.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.AutoHistory.Extensions
{
    internal static class EntityEntryExtensions
    {
        public static string PrimaryKey(this EntityEntry entityEntry)
        {
            var currentValues = new List<object>();

            var key = entityEntry.Metadata.FindPrimaryKey();

            if (key != null)
            {
                foreach (var property in key.Properties)
                {
                    var propertyEntry = entityEntry.Property(property.Name);

                    if (propertyEntry.CurrentValue != null)
                    {
                        currentValues.Add(propertyEntry.CurrentValue);
                    }
                }
            }

            var primaryKey = string.Join(",", currentValues);

            return primaryKey;
        }

        public static TAutoHistory AutoHistory<TAutoHistory>(this EntityEntry entityEntry, EntityState entityState, Func<TAutoHistory> historyFactory) where TAutoHistory : AutoHistory
        {
            var history = historyFactory();
            {
                history.TableName = entityEntry.Metadata.GetTableName();
            }

            // Gets the mapped properties for the entity type
            // Includes shadow properties, but doesn't include navigations and references
            var excludedProperties = entityEntry.Metadata.ClrType.GetProperties()
                                                                 .Where(property => property.GetCustomAttributes(typeof(ExcludeFromHistoryAttribute), inherit: true).Length > 0)
                                                                 .Select(property => property.Name);

            var properties = entityEntry.Properties.Where(entry => !excludedProperties.Contains(entry.Metadata.Name));

            switch (entityState)
            {
                case EntityState.Added:
                    {
                        history = HandleAdded(history, entityEntry, properties);
                    }
                    break;
                case EntityState.Modified:
                    {
                        history = HandleModified(history, entityEntry, properties);
                    }
                    break;
                case EntityState.Deleted:
                    {
                        history = HandleDeleted(history, entityEntry, properties);
                    }
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    {
                        throw new NotSupportedException($"Unsupported '{nameof(EntityState)}'.");
                    }
            }

            return history;
        }

        private static TAutoHistory HandleAdded<TAutoHistory>(TAutoHistory history, EntityEntry entityEntry, IEnumerable<PropertyEntry> properties) where TAutoHistory : AutoHistory
        {
            var json = new JObject();
            var options = AutoHistoryOptions.Instance;

            foreach (var property in properties)
            {
                json[property.Metadata.Name] = property.CurrentValue != null ? JToken.FromObject(property.CurrentValue, options.JsonSerializer) : JValue.CreateNull();
            }

            history.RowId = entityEntry.PrimaryKey();
            history.Kind = EntityState.Added;
            history.Changed = json.ToString(options.JsonSerializerSettings.Formatting);

            return history;
        }

        private static TAutoHistory HandleModified<TAutoHistory>(TAutoHistory history, EntityEntry entityEntry, IEnumerable<PropertyEntry> properties) where TAutoHistory : AutoHistory
        {
            if (!properties.Any(entry => entry.IsModified))
            {
                return null;
            }

            var json = new JObject();

            var before = new JObject();
            var after = new JObject();

            var options = AutoHistoryOptions.Instance;

            PropertyValues databaseValues = null;

            foreach (var property in properties.Where(entry => entry.IsModified))
            {
                if (property.OriginalValue != null)
                {
                    if (!property.OriginalValue.Equals(property.CurrentValue))
                    {
                        before[property.Metadata.Name] = JToken.FromObject(property.OriginalValue, options.JsonSerializer);
                    }
                    else
                    {
                        databaseValues ??= entityEntry.GetDatabaseValues();

                        var originalValue = databaseValues.GetValue<object>(property.Metadata.Name);

                        before[property.Metadata.Name] = originalValue != null ? JToken.FromObject(originalValue, options.JsonSerializer) : JValue.CreateNull();
                    }
                }
                else
                {
                    before[property.Metadata.Name] = JValue.CreateNull();
                }

                after[property.Metadata.Name] = property.CurrentValue != null ? JToken.FromObject(property.CurrentValue, options.JsonSerializer) : JValue.CreateNull();
            }

            json["before"] = before;
            json["after"] = after;

            history.RowId = entityEntry.PrimaryKey();
            history.Kind = EntityState.Modified;
            history.Changed = json.ToString(options.JsonSerializerSettings.Formatting);

            return history;
        }

        private static TAutoHistory HandleDeleted<TAutoHistory>(TAutoHistory history, EntityEntry entityEntry, IEnumerable<PropertyEntry> properties) where TAutoHistory : AutoHistory
        {
            var json = new JObject();
            var options = AutoHistoryOptions.Instance;

            foreach (var property in properties)
            {
                json[property.Metadata.Name] = property.OriginalValue != null ? JToken.FromObject(property.OriginalValue, options.JsonSerializer) : JValue.CreateNull();
            }

            history.RowId = entityEntry.PrimaryKey();
            history.Kind = EntityState.Deleted;
            history.Changed = json.ToString(options.JsonSerializerSettings.Formatting);

            return history;
        }
    }
}
