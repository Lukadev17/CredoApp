namespace CredoApp.MiddleWares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                
                _logger.LogError(ex, "unhandled exception occurred during the request");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;

                var response = new
                {
                    success = false,
                    message = "Internal server error"
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
