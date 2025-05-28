namespace Lotus.Managers.Models;

public class ModdedPlayer
{
    public string HashedPUID { get; set; } = null!;
    public string FriendCode { get; set; } = null!;
    public string ModType { get; set; } = null!;
    public string Name { get; set; } = null!;

    public ModdedPlayer()
    {

    }
}