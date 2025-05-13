using Lotus.API;
using Lotus.Roles.Internals.Enums;

namespace Lotus.Roles.Internals.Attributes;

public class ModifiedActionAttribute : RoleActionAttribute
{
    public ModifiedBehaviour Behaviour = ModifiedBehaviour.Replace;

    public ModifiedActionAttribute(LotusActionType actionType, ActionFlag actionFlags = ActionFlag.None, Priority priority = Priority.Normal) : base(actionType, actionFlags, priority) { }

    public ModifiedActionAttribute(LotusActionType actionType, ModifiedBehaviour behaviour, ActionFlag actionFlags = ActionFlag.None, Priority priority = Priority.Normal) : base(actionType, actionFlags, priority)
    {
        Behaviour = behaviour;
    }
}

public enum ModifiedBehaviour
{

    /// /// <summary>
    /// Replaces any Role Actions of the same type declared within the class
    /// </summary>
    Replace,

    /// <summary>
    ///
    /// </summary>
    PatchBefore,
    PatchAfter,
    Addition
}