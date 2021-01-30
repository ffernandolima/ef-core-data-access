using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.Repository.Collections
{
    public class PagedList<T> : IPagedList<T>
    {
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex - 1 > 0;
        public bool HasNextPage => TotalPages > 0 && PageIndex + 1 <= TotalPages;
        public IList<T> Items { get; set; } = new List<T>();

        public PagedList()
        { }

        public PagedList(IList<T> source, int? pageIndex, int? pageSize, int totalCount)
        {
            if (source?.Any() ?? false)
            {
                PageIndex = pageIndex ?? 1;
                PageSize = pageSize;
                Count = source.Count;
                TotalCount = totalCount;
                TotalPages = PageSize > 0 ? (int)Math.Ceiling((decimal)TotalCount / (decimal)PageSize.Value) : 0;
                Items = source;
            }
        }
    }
}
