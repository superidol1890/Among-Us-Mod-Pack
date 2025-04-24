using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MiraAPI.Utilities;

namespace MiraAPI.Modifiers;

/// <summary>
/// Factory for creating instances of a modifier. More efficient than using reflection.
/// </summary>
public static class ModifierFactory
{
    private static readonly Dictionary<(Type, Type[]), Func<object[], BaseModifier>> _constructorCache = [];

    private static Func<object[], BaseModifier> CreateConstructor(Type type, params object[] args)
    {
        var constructorInfo = type.GetBestConstructor(args) ?? throw new InvalidOperationException(
            $"Could not find a constructor for type {type} with the specified arguments.");

        var parameters = constructorInfo.GetParameters();

        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var constructorParams = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var paramAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            var convertedParam = Expression.Convert(paramAccess, paramType);
            constructorParams[i] = convertedParam;
        }

        var newExpr = Expression.New(constructorInfo, constructorParams);
        var lambda = Expression.Lambda<Func<object[], BaseModifier>>(newExpr, argsParam);

        return lambda.Compile();
    }

    /// <summary>
    /// Creates a modifier with the specified type and arguments.
    /// </summary>
    /// <param name="type">Modifier type.</param>
    /// <param name="args">Arguments.</param>
    /// <returns>An instance of the modifier.</returns>
    public static BaseModifier CreateInstance(Type type, params object[] args)
    {
        var argTypes = args.Select(arg => arg?.GetType() ?? typeof(object)).ToArray();
        var key = (type, argTypes);

        if (!_constructorCache.TryGetValue(key, out var constructor))
        {
            constructor = CreateConstructor(type, args);
            _constructorCache[key] = constructor;
        }

        return constructor(args);
    }
}

/// <summary>
/// Factory for creating instances of a modifier. More efficient than using reflection.
/// </summary>
/// <typeparam name="T">The modifier type.</typeparam>
public static class ModifierFactory<T> where T : BaseModifier
{
    private static readonly Func<object[], T> Constructor = CreateConstructor();

    private static Func<object[], T> CreateConstructor(params object[] args)
    {
        var constructorInfo = typeof(T).GetBestConstructor(args) ?? throw new InvalidOperationException(
            $"Could not find a constructor for type {typeof(T)} with the specified arguments.");

        var parameters = constructorInfo.GetParameters();

        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var constructorParams = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var paramAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            var convertedParam = Expression.Convert(paramAccess, paramType);
            constructorParams[i] = convertedParam;
        }

        var newExpr = Expression.New(constructorInfo, constructorParams);
        var lambda = Expression.Lambda<Func<object[], T>>(newExpr, argsParam);

        return lambda.Compile();
    }

    /// <summary>
    /// Creates an instance of the modifier.
    /// </summary>
    /// <param name="args">Parameters for the constructor.</param>
    /// <returns>An instance of the modifier.</returns>
    public static T CreateInstance(params object[] args)
    {
        return Constructor(args);
    }
}
