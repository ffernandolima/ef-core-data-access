
namespace EntityFrameworkCore.AutoHistory
{
    internal static class AutoHistoryOptionsDefaults
    {
        public const int RowIdMaxLength = 50;
        public const int TableNameMaxLength = 128;
        public const bool LimitChangedLength = true;
        public const int ChangedMaxLength = 2048;
        public const string AutoHistoryTableName = "AutoHistories";
    }
}
