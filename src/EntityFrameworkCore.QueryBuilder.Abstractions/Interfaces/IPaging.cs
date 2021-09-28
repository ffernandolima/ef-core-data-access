
namespace EntityFrameworkCore.QueryBuilder.Interfaces
{
    public interface IPaging
    {
        int? PageIndex { get; }
        int? PageSize { get; }
        int TotalCount { get; }
        bool IsEnabled { get; }
    }
}
