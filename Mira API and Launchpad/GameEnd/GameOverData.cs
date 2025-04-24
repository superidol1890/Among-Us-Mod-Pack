using System.Collections.Generic;

namespace MiraAPI.GameEnd;

/// <summary>
/// Data for a game over.
/// </summary>
/// <param name="Reason">Reason for the game over.</param>
/// <param name="Winners">A collection of winners.</param>
public record struct GameOverData(int Reason, List<NetworkedPlayerInfo> Winners);
