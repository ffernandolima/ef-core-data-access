using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace EntityFrameworkCore.WebAPI.Swagger.Options
{
    /// <summary>
    /// Configures the Swagger generation options.
    /// </summary>
    /// <remarks>This allows API versioning to define a Swagger document per API version after the
    /// <see cref="IApiVersionDescriptionProvider"/> service has been resolved from the service container.</remarks>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
        /// <param name="configuration">The <see cref="IConfiguration">configuration</see> used to get configs from appsettings.json.</param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            // Add a swagger document for each discovered API version
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var applicationName = _configuration["ApplicationName"];

            var openApiInfo = new OpenApiInfo
            {
                Title = applicationName,
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ? $"[Deprecated] - {applicationName}" : applicationName
            };

            return openApiInfo;
        }
    }
}
