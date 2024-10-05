using LandsatReflectance.Backend.Models;
using Target = Org.BouncyCastle.Asn1.X509.Target;

namespace LandsatReflectance.Backend.Services.NotificationSender;

public interface INotificationSender
{
    public void SendNotification(User user, Target target);
}