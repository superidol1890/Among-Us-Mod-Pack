using Lotus.GameModes;

namespace Lotus.Roles.Interfaces;

public interface IRoleComponent
{
    public IRoleComponent Instantiate(GameMode gameMode, PlayerControl player);
}