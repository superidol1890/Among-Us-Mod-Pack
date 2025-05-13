using System;

namespace Lotus.Roles.Exceptions;

public class NoSuchRoleException : Exception
{
    public NoSuchRoleException(string message) : base(message) { }
}