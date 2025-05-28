using System;

namespace Lotus.Roles.Exceptions;

public class DuplicateRoleIdException : Exception
{
    public CustomRole Original { get; }
    public CustomRole Conflict { get; }

    public DuplicateRoleIdException(object id, CustomRole original, CustomRole conflict) :
        base($"Duplicate RoleID ({id}) between original=\"{original.GetType().FullName}\" and conflict=\"{conflict.GetType().FullName}\"")
    {
        Original = original;
        Conflict = conflict;
    }
}