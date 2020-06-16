
namespace EntityFrameworkCore.QueryBuilder.Interfaces
{
    public interface IQueryFactory<T> where T : class
    {
        ISingleResultQuery<T> SingleResultQuery();
        IMultipleResultQuery<T> MultipleResultQuery();

        ISingleResultQuery<T, TResult> SingleResultQuery<TResult>();
        IMultipleResultQuery<T, TResult> MultipleResultQuery<TResult>();
    }
}
