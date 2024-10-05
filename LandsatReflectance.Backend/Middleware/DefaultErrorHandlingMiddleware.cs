namespace LandsatReflectance.Backend.Middleware;

public class DefaultErrorHandlingMiddleware
{
    private readonly RequestDelegate m_next;
    private readonly ILogger<DefaultErrorHandlingMiddleware> m_logger;

    public DefaultErrorHandlingMiddleware(RequestDelegate next, ILogger<DefaultErrorHandlingMiddleware> logger)
    {
        m_next = next;
        m_logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await m_next(httpContext);
        }
        catch (Exception exception)
        {
            m_logger.LogError(exception, "An unhandled exception occurred.");
            // await HandleExceptionAsync(httpContext, exception);
        }
    }

    public static Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        // Maybe add some extra stuff here idk
        return Task.CompletedTask;
    }
}