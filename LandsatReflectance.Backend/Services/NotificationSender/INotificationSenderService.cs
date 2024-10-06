using LandsatReflectance.Backend.Models;

namespace LandsatReflectance.Backend.Services.NotificationSender;

public interface INotificationSenderService
{
    public void SendNotification(User user, Target target);
}