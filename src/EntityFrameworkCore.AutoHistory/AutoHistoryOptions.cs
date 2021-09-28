using EntityFrameworkCore.AutoHistory.Serialization;
using Newtonsoft.Json;
using System;

namespace EntityFrameworkCore.AutoHistory
{
    public sealed class AutoHistoryOptions
    {
        private static readonly Lazy<AutoHistoryOptions> AutoHistoryOptionsFactory = new(() => new AutoHistoryOptions(), isThreadSafe: true);

        public static AutoHistoryOptions Instance => AutoHistoryOptionsFactory.Value;

        private AutoHistoryOptions()
        { }

        public int? ChangedMaxLength { get; set; }
        public bool LimitChangedLength { get; set; } = true;
        public int RowIdMaxLength { get; set; } = 50;
        public int TableMaxLength { get; set; } = 128;
        internal JsonSerializer JsonSerializer { get; set; }
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = AutoHistorySerialization.DefaultSettings;
    }
}
