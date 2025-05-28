using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;
using System.Linq.Expressions;

namespace Lotus.Extensions;

public static class ObjectExtensions
{
    public static bool TryCast<T>(this Il2CppObjectBase obj, [MaybeNullWhen(false)] out T casted)
    where T : Il2CppObjectBase
    {
        casted = obj.TryCast<T>();
        return casted != null;
    }

    public static bool HasParentInHierarchy(this GameObject obj, string parentPath)
    {
        string[] pathParts = parentPath.Split('/');
        int pathIndex = pathParts.Length - 1;

        Transform current = obj.transform;

        while (current != null)
        {
            if (current.name == pathParts[pathIndex])
            {
                pathIndex--;
                if (pathIndex < 0) return true;
            }
            else pathIndex = pathParts.Length - 1;
            current = current.parent;
        }

        return false;
    }

    public static T CastFast<T>(this Il2CppObjectBase obj) where T : Il2CppObjectBase
    {
        if (obj is T casted) return casted;
        return obj.Pointer.CastFast<T>();
    }

    private static T CastFast<T>(this IntPtr ptr) where T : Il2CppObjectBase
    {
        return CastHelper<T>.Cast(ptr);
    }

    private static class CastHelper<T> where T : Il2CppObjectBase
    {
        public static readonly Func<IntPtr, T> Cast;

        static CastHelper()
        {
            var constructor = typeof(T).GetConstructor([typeof(IntPtr)]);
            var ptr = Expression.Parameter(typeof(IntPtr));
            var create = Expression.New(constructor!, ptr);
            var lambda = Expression.Lambda<Func<IntPtr, T>>(create, ptr);
            Cast = lambda.Compile();
        }
    }
}