using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Default: assume protected (global requirement applies)
        bool requiresAuth = true;

        // Check for explicit [AllowAnonymous] on method / metadata (MVC + Minimal)
        bool hasAllowAnonymous =
            context.MethodInfo?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() == true ||
            context.ApiDescription?.ActionDescriptor?.EndpointMetadata?.OfType<AllowAnonymousAttribute>().Any() == true;

        if (hasAllowAnonymous)
        {
            requiresAuth = false;
        }
        else
        {
            // Check for authorization indicators (protect unless explicitly anonymous)
            bool hasAuthMetadata = false;

            // MVC style [Authorize]
            hasAuthMetadata |=
                context.MethodInfo?.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true ||
                context.MethodInfo?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

            var endpointMetadata = context.ApiDescription?.ActionDescriptor?.EndpointMetadata;
            if (endpointMetadata != null)
            {
                // .RequireAuthorization() adds IAuthorizeData (often AuthorizeAttribute or policy metadata)
                hasAuthMetadata |= endpointMetadata.OfType<IAuthorizeData>().Any();
                hasAuthMetadata |= endpointMetadata.OfType<AuthorizeAttribute>().Any();

                // Extra safety: check for common policy/requirement metadata types
                hasAuthMetadata |= endpointMetadata.Any(m => m?.GetType().FullName?.Contains("Authorize", StringComparison.OrdinalIgnoreCase) == true);
            }

            requiresAuth = hasAuthMetadata;
        }

        if (!requiresAuth)
        {
            // Explicitly clear security for anonymous endpoints → no lock, no header sent
            operation.Security = new List<OpenApiSecurityRequirement>();
            // Optional: operation.Description += " (Public - no auth required)";
        }
        // else: keep the global/default security → lock icon + header sent
    }
}