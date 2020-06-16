using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.QueryBuilder
{
    public class Sorting<T> : Sorting
    {
        internal Sorting()
        { }

        public Expression<Func<T, object>> KeySelector { get; internal set; }
    }

    public class Sorting
    {
        internal Sorting()
        { }

        public string FieldName { get; internal set; }
        public SortDirection SortDirection { get; internal set; }
    }
}
