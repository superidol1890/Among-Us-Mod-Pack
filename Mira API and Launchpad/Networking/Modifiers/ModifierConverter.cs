using System;
using Hazel;
using InnerNet;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Serialization;

namespace MiraAPI.Networking.Modifiers;

/// <summary>
/// Converter for serializing and deserializing <see cref="BaseModifier"/> objects.
/// </summary>
[MessageConverter]
public class ModifierConverter : MessageConverter<BaseModifier>
{
    /// <summary>
    /// Writes a <see cref="BaseModifier"/> to the writer.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The <see cref="BaseModifier"/> to write.</param>
    public override void Write(MessageWriter writer, BaseModifier value)
    {
        writer.WriteNetObject(value.Player);
        writer.WriteBytesAndSize(value.UniqueId.ToByteArray());
    }

    /// <summary>
    /// Reads a <see cref="BaseModifier"/> from the reader.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="objectType">The type of the object to read.</param>
    /// <returns>The <see cref="BaseModifier"/> that was read.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the modifier is not found.</exception>
    public override BaseModifier Read(MessageReader reader, Type objectType)
    {
        var player = reader.ReadNetObject<PlayerControl>();
        var guid = new Guid(reader.ReadBytesAndSize());
        return player.GetModifier(guid) ?? throw new InvalidOperationException($"Modifier with GUID {guid} not found for player with ID: {player.PlayerId}");
    }
}
