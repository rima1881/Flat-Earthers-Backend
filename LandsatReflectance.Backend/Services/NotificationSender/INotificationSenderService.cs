using LandsatReflectance.Backend.Models;

namespace LandsatReflectance.Backend.Services.NotificationSender;

public interface INotificationSenderService
{
    public void SendTargetNotification(User user, Target target);
    
    public void SendGeneralNotification(string receiverInformation, string message, string? subject);
}