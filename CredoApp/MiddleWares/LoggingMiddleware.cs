using Serilog;

namespace CredoApp.MiddleWares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            
            Log.Information($"HTTP Request: {context.Request.Method} {context.Request.Path}");

            try
            {
                await _next(context); 

                
                Log.Information($"HTTP Response: {context.Response.StatusCode} for {context.Request.Path}");
            }
            catch (Exception ex)
            {
                
                Log.Error(ex, $"Unhandled Exception occurred during: {context.Request.Method} {context.Request.Path}");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { message = "Error" });
            }
        }
    }
}
