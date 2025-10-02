using Hazel;
using ModPack.Networking.Attributes;
using ModPack.Networking.Rpc;

namespace ModPack;

[RegisterCustomRpc((uint) CustomRpcCalls.ModPack)]
public class ModPackRpc : PlayerCustomRpc<ModPackPlugin, ModPackRpc.Data>
{
    public ModPackRpc(ModPackPlugin plugin, uint id) : base(plugin, id)
    {
    }

    public readonly record struct Data(string Message);

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Write(MessageWriter writer, Data data)
    {
        writer.Write(data.Message);
    }

    public override Data Read(MessageReader reader)
    {
        return new Data(reader.ReadString());
    }

    public override void Handle(PlayerControl innerNetObject, Data data)
    {
        Plugin.Log.LogWarning($"Handle: {innerNetObject.Data.PlayerName} sent \"{data.Message}\"");
    }
}
