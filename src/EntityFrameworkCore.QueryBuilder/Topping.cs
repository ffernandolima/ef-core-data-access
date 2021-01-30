
namespace EntityFrameworkCore.QueryBuilder
{
    public class Topping
    {
        internal Topping()
        { }

        public int? TopRows { get; internal set; }
        public bool IsEnabled => TopRows > 0;
    }
}
