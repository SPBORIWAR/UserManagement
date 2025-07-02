namespace UserManagement.Application.Helper
{
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Linq;

    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check for authorize attribute
            var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                                    .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any()
                               || context.MethodInfo.GetCustomAttributes(true)
                                    .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();

            if (hasAuthorize)
            {
                operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [ new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }
                    ] = new List<string>()
                }
            };
            }
        }
    }

}
