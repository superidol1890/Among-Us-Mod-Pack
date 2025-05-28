using System.IO;
using System.Linq;
using System.Text;
using Lotus.Extensions;
using Lotus.Managers.Models;
using VentLib.Utilities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lotus.Managers;

public class ModManager
{
    private IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    private ISerializer serializer = new SerializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    private ModPlayerFile modPlayerFile;
    private FileInfo moddedPlayers;

    public ModManager(FileInfo moddedPlayers)
    {
        this.moddedPlayers = moddedPlayers;

        if (!moddedPlayers.Exists) WriteFile(new ModPlayerFile());

        string content;
        using (StreamReader reader = new(moddedPlayers.Open(FileMode.OpenOrCreate))) content = reader.ReadToEnd();
        modPlayerFile = deserializer.Deserialize<ModPlayerFile>(content);
    }

    public void ModPlayer(PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (target.FriendCode == "") return;
        if (IsPlayerModded(target)) return;

        modPlayerFile.Players.Add(new ModdedPlayer()
        {
            Name = target.name,
            FriendCode = target.FriendCode,
            HashedPUID = target.GetClient()?.GetHashedPuid() ?? "",
            ModType = "Mod"
        });
        WriteFile(modPlayerFile);
    }

    public void AdminPlayer(PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (target.FriendCode == "") return;
        if (IsPlayerModded(target)) return;

        modPlayerFile.Players.Add(new ModdedPlayer()
        {
            Name = target.name,
            FriendCode = target.FriendCode,
            HashedPUID = target.GetClient()?.GetHashedPuid() ?? "",
            ModType = "Admin"
        });
        WriteFile(modPlayerFile);
    }

    public bool IsPlayerModded(PlayerControl target) => GetStatusOfPlayer(target) is not NotModded;

    public void UnmodPlayer(PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!IsPlayerModded(target)) return;
        modPlayerFile.Players.RemoveAll(mp => mp.FriendCode == target.FriendCode);
        WriteFile(modPlayerFile);
    }

    public ModdedPlayer GetStatusOfPlayer(PlayerControl target) => modPlayerFile.Players.FirstOrDefault(mp => mp.FriendCode == target.FriendCode, new NotModded());

    private void WriteFile(ModPlayerFile modPlayerFile)
    {
        string emptyFile = serializer.Serialize(modPlayerFile);
        using FileStream stream = moddedPlayers.Open(FileMode.Create);
        stream.Write(Encoding.UTF8.GetBytes(emptyFile));
    }
}

public class NotModded : ModdedPlayer
{
    public NotModded()
    {
        FriendCode = "null";
        Name = "null";
        HashedPUID = "null";
        ModType = "Not Modded";
    }
}