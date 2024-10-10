using LandsatReflectance.Backend.Services;
using LandsatReflectance.Backend.Services.NotificationSender;

namespace LandsatReflectance.Backend.Middleware;

public class DefaultErrorHandlingMiddleware
{
    private readonly RequestDelegate m_next;
    private readonly ILogger<DefaultErrorHandlingMiddleware> m_logger;

    private readonly IServiceScopeFactory m_serviceScopeFactory;
    private readonly List<string> m_emailsToSendOnUnhandledError;

    public DefaultErrorHandlingMiddleware(
        RequestDelegate next, 
        ILogger<DefaultErrorHandlingMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory,
        KeysService keysService)
    {
        m_next = next;
        m_logger = logger;
        m_serviceScopeFactory = serviceScopeFactory;

        m_emailsToSendOnUnhandledError = keysService.EmailsToNotifyOnError;
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
            await SendExceptionInformation(httpContext, exception);
        }
    }

    private Task SendExceptionInformation(HttpContext httpContext, Exception exception)
    {
        using var serviceScope = m_serviceScopeFactory.CreateScope();
        var notificationSenderServices = serviceScope.ServiceProvider.GetServices<INotificationSenderService>().ToList();
        
        var emailContents = $"""
                  An unhandled exception happened on the backend.
                  
                  Date: {DateTime.UtcNow}
                  
                  Machine Information:
                  Machine Name: {Environment.MachineName}
                  OS Version: {Environment.OSVersion}
                  DOTNET version: {Environment.Version}
                  
                  HttpContext Information:
                  Server Name: {httpContext.Request.Host.Host}
                  Server IP Address: {httpContext.Connection.LocalIpAddress}
                  
                  Exception Information:
                  Exception message: {exception.Message}
                  Exception stack trace: {exception.StackTrace}
                  """;

        foreach (var notificationSenderService in notificationSenderServices)
        {
            foreach (var email in m_emailsToSendOnUnhandledError)
            {
                try
                {
                    var subject = "An unhandled exception happened on the backend";
                    notificationSenderService.SendGeneralNotification(email, emailContents, subject);
                }
                catch
                {
                    // fuck it
                }
            }
        }
        
        
        return Task.CompletedTask;
    }
}