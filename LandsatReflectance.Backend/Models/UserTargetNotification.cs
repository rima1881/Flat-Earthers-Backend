namespace LandsatReflectance.Backend.Models;

public class UserTargetNotification
{
    public int Path { get; set; }
    public int Row { get; set; }
    public Guid UserGuid { get; set; } = new Guid();
    public Guid TargetGuid { get; set; } = new Guid();
    public DateTime PredictedAcquisitionDate { get; set; } = DateTime.MinValue; 
    public bool HasBeenNotified { get; set; }
}