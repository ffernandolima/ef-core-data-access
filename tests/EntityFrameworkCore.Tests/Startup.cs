using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EntityFrameworkCore.Tests
{
    public class Startup : IStartup
    {
        protected IServiceCollection Services { get; private set; }
        protected IConfiguration Configuration { get; private set; }

        private IServiceProvider _serviceProvider;
        protected IServiceProvider ServiceProvider => (_serviceProvider = _serviceProvider ?? Services?.BuildServiceProvider());

        public Startup(bool configure = true)
        {
            Services = new ServiceCollection();

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            if (configure)
            {
                ConfigureServices(Services);
            }
        }

        public virtual void ConfigureServices(IServiceCollection services)
        { }
    }
}
