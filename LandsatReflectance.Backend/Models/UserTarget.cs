namespace LandsatReflectance.Backend.Models;

// Class representing the join table between 'User' and 'Targets' in the db.

public class UserTarget
{
    public Guid UserGuid { get; set; } = Guid.NewGuid();
    public Guid TargetGuid { get; set; } = Guid.NewGuid();

    public UserTarget()
    { }

    public UserTarget(Guid userGuid, Guid targetGuid)
    {
        UserGuid = userGuid;
        TargetGuid = targetGuid;
    }
}