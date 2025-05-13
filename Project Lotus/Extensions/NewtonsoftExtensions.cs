using Newtonsoft.Json.Linq;

namespace Lotus.Extensions;

public static class NewtonsoftExtensions
{
    public static T Value<T>(this JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
            return default!;

        object obj = token.ToObject(Il2CppSystem.Type.GetType(typeof(T).AssemblyQualifiedName));
        return (T)obj;
    }
}