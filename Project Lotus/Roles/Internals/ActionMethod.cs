#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.Internals;


public static class MethodInfoExtension
{
    public static object? InvokeAligned(this MethodInfo info, object obj, params object[] parameters)
    {
        try
        {
            return info.Invoke(obj, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, AlignFunctionParameters(info, parameters), null);
        }
        catch (TargetException) { throw; }
        catch (Exception e)
        {
            string fullName = $"{info.ReflectedType?.FullName}.{info.Name}({string.Join(",", info.GetParameters().Select(o => $"{o.ParameterType} {o.Name}").ToArray())})";
            throw new Exception($"Failed to align parameters for method \"{fullName}\". | Parameters = {parameters.Fuse()}", e);
        }
    }


    private static object[] AlignFunctionParameters(MethodInfo method, IEnumerable<object?> allParameters)
    {
        List<object?> allParametersList = allParameters.ToList();
        List<object> functionSpecificParameters = new();

        int i = 1;
        foreach (ParameterInfo parameter in method.GetParameters())
        {
            int matchingParamIndex = allParametersList.FindIndex(obj => obj != null && obj.GetType().IsAssignableTo(parameter.ParameterType));
            if (matchingParamIndex == -1 && !parameter.IsOptional)
                throw new ArgumentException($"Invocation of {method.Name} does not contain all required arguments. Argument {i} ({parameter.Name}) was not supplied.");
            functionSpecificParameters.Add(allParametersList.Pop(matchingParamIndex)!);
            i++;
        }

        return functionSpecificParameters.ToArray();
    }
}
