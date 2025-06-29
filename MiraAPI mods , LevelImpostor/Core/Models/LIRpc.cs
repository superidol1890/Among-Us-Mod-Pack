﻿namespace LevelImposter.Core;
// Among Us     0 - 61
// TOR          60 - 73, 100 - 149
// Las Monjas   60 - 69, 75 - 194
// StellaRoles  60 - 169
// ToU          100 - 210, 220 - 251
// Submerged    210 - 214

// LI           90 - 99 (Wow we really need to coordinate this better)

public enum LIRpc
{
    FireTrigger = 90, // Fires a global trigger on an object
    UpdateObjectPos, // Used by util-physics object
    SyncMapID, // Syncs the map ID in the lobby
    SyncRandomSeed, // Syncs a random seed for util-triggerrand
    ResetPlayer, // Resets the player on Ctrl-R E S
    DownloadCheck, // Warns the host that the client is still downloading the map
    KillPlayer, // Used by util-triggerdeath object
    Reserved97,
    Reserved98,
    Reserved99
}