using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Text.Json.Nodes;

namespace EntityFrameworkCore.WebAPI.Swagger.Filters
{
    /// <summary>
    /// Represents the Swagger/Swashbuckle operation filter used to document the implicit API version parameter.
    /// </summary>
    /// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
    /// Once they are fixed and published, this class can be removed.</remarks>
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters is null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters.OfType<OpenApiParameter>())
            {
                var description = apiDescription.ParameterDescriptions.FirstOrDefault(p => string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

                if (description is null)
                {
                    continue;
                }

                if (parameter.Description is null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema is OpenApiSchema schema && schema.Default is null && description.DefaultValue is not null)
                {
                    schema.Default = JsonValue.Create(description.DefaultValue.ToString());
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}
