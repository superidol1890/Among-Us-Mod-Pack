using Lotus.Extensions;
using UnityEngine;

namespace Lotus.RPC.CustomObjects.Builtin;

public sealed class FakePlayer : CustomNetObject
{
    public NetworkedPlayerInfo.PlayerOutfit outfitToCopy;
    public byte OwnerId;
    public FakePlayer(NetworkedPlayerInfo.PlayerOutfit outfitToCopy, Vector2 position, byte OwnerId)
    {
        this.OwnerId = OwnerId;
        this.outfitToCopy = outfitToCopy.DeepCopy();
        CreateNetObject(this.outfitToCopy.PlayerName, position);
    }
    public override void SetupOutfit()
    {
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = outfitToCopy.PlayerName;
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = outfitToCopy.ColorId;
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = outfitToCopy.HatId;
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = outfitToCopy.SkinId;
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = outfitToCopy.PetId;
        PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = outfitToCopy.VisorId;
        this.playerControl.RawSetColor(outfitToCopy.ColorId);
    }
    public override bool CanTarget() => true;
}