using EntityFrameworkCore.QueryBuilder.Interfaces;

namespace EntityFrameworkCore.QueryBuilder
{
    public class Topping : ITopping
    {
        internal Topping()
        { }

        public int? TopRows { get; internal set; }
        public bool IsEnabled => TopRows > 0;
    }
}
