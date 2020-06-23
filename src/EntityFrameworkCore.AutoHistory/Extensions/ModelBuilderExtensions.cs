using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;

namespace EntityFrameworkCore.AutoHistory.Extensions
{
    public static class ModelBuilderExtensions
    {
        private const int DefaultChangedMaxLength = 2048;

        public static ModelBuilder EnableAutoHistory(this ModelBuilder modelBuilder, int? changedMaxLength = null)
            => EnableAutoHistory<AutoHistory>(modelBuilder, options => { options.ChangedMaxLength = changedMaxLength; options.LimitChangedLength = false; });

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
                                 .HasMaxLength(options.TableMaxLength);

                if (options.LimitChangedLength)
                {
                    var maxLength = options.ChangedMaxLength ?? DefaultChangedMaxLength;

                    if (maxLength <= 0)
                    {
                        maxLength = DefaultChangedMaxLength;
                    }

                    entityTypeBuilder.Property(autoHistory => autoHistory.Changed)
                                     .HasMaxLength(maxLength);
                }
            });

            return modelBuilder;
        }
    }
}
