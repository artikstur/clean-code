using System.Text;

namespace API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Получен запрос: {method} {url}", context.Request.Method, context.Request.Path);
        _logger.LogInformation("Заголовки запроса: {headers}", context.Request.Headers);
        
        context.Request.EnableBuffering();
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            _logger.LogInformation("Тело запроса: {body}", body);
            context.Request.Body.Position = 0; 
        }

        await _next(context);
    }
}