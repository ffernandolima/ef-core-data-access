using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Tests
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection services);
    }
}
