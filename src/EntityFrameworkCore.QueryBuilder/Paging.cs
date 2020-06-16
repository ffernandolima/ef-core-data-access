
namespace EntityFrameworkCore.QueryBuilder
{
    public class Paging
    {
        internal Paging()
        { }

        public int? PageIndex { get; internal set; }
        public int? PageSize { get; internal set; }
        public int TotalCount { get; internal set; }
        public bool IsEnabled => PageSize.HasValue && PageSize.Value > 0;
    }
}
