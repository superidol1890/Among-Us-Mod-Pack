using System;
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Serialization;

namespace MiraAPI.Networking;

/// <summary>
/// Converter for <see cref="Guid"/>.
/// </summary>
[MessageConverter]
public class GuidConverter : MessageConverter<Guid>
{
    /// <summary>
    /// Writes a <see cref="Guid"/> to the writer.
    /// </summary>
    /// <inheritdoc />
    public override void Write(MessageWriter writer, Guid value)
    {
        writer.WriteBytesAndSize(value.ToByteArray());
    }

    /// <summary>
    /// Reads a <see cref="Guid"/> from the reader.
    /// </summary>
    /// <inheritdoc />
    /// <returns>A GUID object.</returns>
    public override Guid Read(MessageReader reader, Type objectType)
    {
        return new Guid(reader.ReadBytesAndSize());
    }
}
