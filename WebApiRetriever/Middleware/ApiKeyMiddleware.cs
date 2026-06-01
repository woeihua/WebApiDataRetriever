using Microsoft.Extensions.Configuration;

namespace WebApiRetriever.Middleware
{

    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            var validKeys = configuration.GetSection("ApiConfig:ValidKeys").Get<List<string>>();

            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey)
                || validKeys == null || !validKeys.Contains(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid or Missing API Key");
                return;
            }
            await _next(context);
        }
    }
}
