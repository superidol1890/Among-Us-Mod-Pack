using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System;
using System.IO;
using YamlDotNet.Core;

namespace Lotus.Managers.Announcements.Helpers;

public class DateOnlyYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(DateOnly);
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        var value = parser.Consume<YamlDotNet.Core.Events.Scalar>().Value;
        return DateOnly.ParseExact(value, "yyyy-MM-dd");
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var date = (DateOnly)value!;
        emitter.Emit(new YamlDotNet.Core.Events.Scalar(date.ToString("yyyy-MM-dd")));
    }
}