namespace LaunchpadReloaded.Roles.Afterlife;

public interface IAfterlifeRole
{
    public bool CanBeAssigned(PlayerControl player)
    {
        return true;
    }
}
