using EntityFrameworkCore.AutoHistory.Serialization;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace EntityFrameworkCore.AutoHistory
{
    public sealed class AutoHistoryOptions
    {
        private static readonly Lazy<AutoHistoryOptions> AutoHistoryOptionsFactory = new(() => new AutoHistoryOptions(), isThreadSafe: true);

        public static AutoHistoryOptions Instance => AutoHistoryOptionsFactory.Value;

        private AutoHistoryOptions()
        { }

        public int RowIdMaxLength { get; set; } = AutoHistoryOptionsDefaults.RowIdMaxLength;
        public int TableNameMaxLength { get; set; } = AutoHistoryOptionsDefaults.TableNameMaxLength;
        public bool LimitChangedLength { get; set; } = AutoHistoryOptionsDefaults.LimitChangedLength;
        public int? ChangedMaxLength { get; set; }
        public string AutoHistoryTableName { get; set; } = AutoHistoryOptionsDefaults.AutoHistoryTableName;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal JsonSerializer JsonSerializer { get; set; }
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = AutoHistorySerialization.DefaultSettings;
    }
}
