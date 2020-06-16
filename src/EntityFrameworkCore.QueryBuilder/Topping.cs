
namespace EntityFrameworkCore.QueryBuilder
{
    public class Topping
    {
        internal Topping()
        { }

        public int? TopRows { get; internal set; }
        public bool IsEnabled => TopRows.HasValue && TopRows.Value > 0;
    }
}
