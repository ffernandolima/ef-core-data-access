using EntityFrameworkCore.QueryBuilder.Interfaces;

namespace EntityFrameworkCore.QueryBuilder
{
    public class Paging : IPaging
    {
        internal Paging()
        { }

        public int? PageIndex { get; internal set; }
        public int? PageSize { get; internal set; }
        public int TotalCount { get; internal set; }
        public bool IsEnabled => PageSize > 0;
    }
}
