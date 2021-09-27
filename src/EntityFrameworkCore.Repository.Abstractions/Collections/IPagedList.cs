using System.Collections.Generic;

namespace EntityFrameworkCore.Repository.Collections
{
    public interface IPagedList<T>
    {
        int? PageIndex { get; }
        int? PageSize { get; }
        int Count { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        IList<T> Items { get; }
    }
}
