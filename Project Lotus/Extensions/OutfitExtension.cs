namespace Lotus.Extensions;

public static class OutfitExtension
{
    public static NetworkedPlayerInfo.PlayerOutfit Clone(this NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        NetworkedPlayerInfo.PlayerOutfit copied = new()
        {
            PlayerName = outfit.PlayerName,
            ColorId = outfit.ColorId,
            HatId = outfit.HatId,
            PetId = outfit.PetId,
            SkinId = outfit.SkinId,
            VisorId = outfit.VisorId,
            NamePlateId = outfit.NamePlateId
        };
        return copied;
    }
}