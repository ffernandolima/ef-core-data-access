using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EntityFramewrokCore.WebAPI
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());
    }
}
