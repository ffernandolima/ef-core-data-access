using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;

namespace EntityFrameworkCore.AutoHistory.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder EnableAutoHistory(this ModelBuilder modelBuilder, int? changedMaxLength = null)
            => EnableAutoHistory<AutoHistory>(modelBuilder, options => { options.ChangedMaxLength = changedMaxLength; options.LimitChangedLength = false; });

        public static ModelBuilder EnableAutoHistory<TAutoHistory>(this ModelBuilder modelBuilder, int? changedMaxLength = null) where TAutoHistory : AutoHistory
            => EnableAutoHistory<TAutoHistory>(modelBuilder, options => { options.ChangedMaxLength = changedMaxLength; options.LimitChangedLength = false; });

        public static ModelBuilder EnableAutoHistory<TAutoHistory>(this ModelBuilder modelBuilder, Action<AutoHistoryOptions> configure) where TAutoHistory : AutoHistory
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder), $"{nameof(modelBuilder)} cannot be null.");
            }

            var options = AutoHistoryOptions.Instance;

            configure?.Invoke(options);

            options.JsonSerializer = JsonSerializer.Create(options.JsonSerializerSettings);

            modelBuilder.Entity<TAutoHistory>(entityTypeBuilder =>
            {
                entityTypeBuilder.Property(autoHistory => autoHistory.RowId)
                                 .IsRequired()
                                 .HasMaxLength(options.RowIdMaxLength);

                entityTypeBuilder.Property(autoHistory => autoHistory.TableName)
                                 .IsRequired()
                                 .HasMaxLength(options.TableNameMaxLength);

                if (options.LimitChangedLength)
                {
                    var changedMaxLength = options.ChangedMaxLength.GetValueOrDefault();

                    if (changedMaxLength <= 0)
                    {
                        changedMaxLength = AutoHistoryOptionsDefaults.ChangedMaxLength;
                    }

                    entityTypeBuilder.Property(autoHistory => autoHistory.Changed)
                                     .HasMaxLength(changedMaxLength);
                }

                var autoHistoryTableName = options.AutoHistoryTableName;

                if (string.IsNullOrWhiteSpace(autoHistoryTableName))
                {
                    autoHistoryTableName = AutoHistoryOptionsDefaults.AutoHistoryTableName;
                }

                entityTypeBuilder.ToTable(autoHistoryTableName);
            });

            return modelBuilder;
        }
    }
}
