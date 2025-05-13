using AmongUs.GameOptions;

namespace Lotus.Extensions;

public static class RoleTypesExtension
{
    public static bool IsImpostor(this RoleTypes roleTypes) => roleTypes is RoleTypes.Impostor or RoleTypes.Shapeshifter or RoleTypes.ImpostorGhost or RoleTypes.Phantom;
    public static bool IsCrewmate(this RoleTypes roleTypes) => !IsImpostor(roleTypes);

    public static RoleTypes GhostEquivelant(this RoleTypes roleTypes) => roleTypes.IsImpostor() ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;
}