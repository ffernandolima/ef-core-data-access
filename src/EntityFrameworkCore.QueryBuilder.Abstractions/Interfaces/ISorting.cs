using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder.Interfaces
{
    public interface ISorting<T> : ISorting
    {
        Expression<Func<T, object>> KeySelector { get; }
    }

    public interface ISorting
    {
        string FieldName { get; }
        SortDirection SortDirection { get; }
    }
}
