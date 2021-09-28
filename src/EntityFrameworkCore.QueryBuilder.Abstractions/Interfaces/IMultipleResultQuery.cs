
namespace EntityFrameworkCore.QueryBuilder.Interfaces
{
    public interface IMultipleResultQuery
    {
        IPaging Paging { get; }
        ITopping Topping { get; }
    }

    public interface IMultipleResultQuery<T> : IMultipleResultQuery, IQuery<T> where T : class
    {
        IMultipleResultQuery<T> Page(int? pageIndex, int? pageSize);
        IMultipleResultQuery<T> Top(int? topRows);
    }

    public interface IMultipleResultQuery<T, TResult> : IMultipleResultQuery, IQuery<T, TResult> where T : class
    {
        IMultipleResultQuery<T, TResult> Page(int? pageIndex, int? pageSize);
        IMultipleResultQuery<T, TResult> Top(int? topRows);
    }
}
