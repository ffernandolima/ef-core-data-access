using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.UnitOfWork.Extensions;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using EntityFrameworkCore.WebAPI.Swagger.Filters;
using EntityFrameworkCore.WebAPI.Swagger.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace EntityFrameworkCore.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                // Format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            var connectionString = Configuration.GetConnectionString("EFCoreDataAccess");

            // SQL Server
            services.AddDbContext<BloggingContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    var assembly = typeof(BloggingContext).Assembly;
                    var assemblyName = assembly.GetName();

                    sqlServerOptions.MigrationsAssembly(assemblyName.Name);
                });

                options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            });

            // PostgreSQL
            // services.AddDbContext<BloggingContext>(options =>
            // {
            //     options.UseNpgsql(connectionString, NpgsqlOptions =>
            //     {
            //         var assembly = typeof(BloggingContext).Assembly;
            //         var assemblyName = assembly.GetName();
               
            //         NpgsqlOptions.MigrationsAssembly(assemblyName.Name);
            //     });
               
            //     options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            // });

            // MySQL
            // services.AddDbContext<BloggingContext>(options =>
            // {
            //     options.UseMySQL(connectionString, mySqlOptions =>
            //     {
            //         var assembly = typeof(BloggingContext).Assembly;
            //         var assemblyName = assembly.GetName();
               
            //         mySqlOptions.MigrationsAssembly(assemblyName.Name);
            //     });
               
            //     options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            // });

            services.AddScoped<DbContext, BloggingContext>();
            services.AddUnitOfWork();
            services.AddUnitOfWork<BloggingContext>();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllParametersInCamelCase();
                options.OperationFilter<SwaggerDefaultValues>();

                var appPath = AppDomain.CurrentDomain.BaseDirectory;

                var entryAssembly = Assembly.GetEntryAssembly();
                var aseemblyName = entryAssembly?.GetName();
                var appName = aseemblyName?.Name;

                var filePath = Path.Combine(appPath, $"{appName ?? "EntityFrameworkCore.WebAPI"}.xml");

                options.IncludeXmlComments(filePath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"API {description.GroupName.ToUpperInvariant()}");
                }
            });

            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

            using (var serviceScope = serviceScopeFactory.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<BloggingContext>();

                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var unitOfWork = serviceScope.ServiceProvider.GetService<IUnitOfWork>();
                var repository = unitOfWork.Repository<Blog>();

                if (!repository.Any())
                {
                    var blogs = Seeder.SeedBlogs();

                    repository.AddRange(blogs);
                    unitOfWork.SaveChanges();
                }
            }
        }
    }
}
