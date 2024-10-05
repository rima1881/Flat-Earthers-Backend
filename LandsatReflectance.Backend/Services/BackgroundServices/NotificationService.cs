namespace LandsatReflectance.Backend.Services.BackgroundServices;

public class NotificationService : BackgroundService
{
    private readonly TimeSpan m_checkInterval = TimeSpan.FromMinutes(10);
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("Hello world from notification service!");
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception exception)
            {
                
            }
            
            await Task.Delay(m_checkInterval, stoppingToken);
        }
    }
}