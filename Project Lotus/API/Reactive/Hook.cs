using System;

namespace Lotus.API.Reactive;

// ReSharper disable once InconsistentNaming
public interface Hook<T>: Hook where T: IHookEvent
{
    /// <summary>
    /// Checks and potentially returns event consumer bound to given key. Returns true if consumer exists, otherwise false.
    /// </summary>
    /// <param name="key">key of binding</param>
    /// <param name="eventConsumer">if present, the bound event consumer, otherwise null</param>
    /// <returns>true if consumer exists, otherwise false</returns>
    bool TryGet(string key, out Action<T>? eventConsumer);

    /// <summary>
    /// Checks and potentially returns event consumer bound to given key. Returns true if consumer exists, otherwise false.
    /// </summary>
    /// <param name="key">key of binding</param>
    /// <param name="eventConsumer">if present, the bound event consumer, otherwise null</param>
    /// <returns>true if consumer exists, otherwise false</returns>
    bool TryGet(string key, out HookAction<T>? eventConsumer);

    /// <summary>
    /// Binds an event consumer to the hook under the given key, if a consumer already exists under the given key, and replace is false
    /// throws an exception
    /// </summary>
    /// <param name="key">key to bind consumer to</param>
    /// <param name="eventConsumer">a function taking in a <see cref="IHookEvent"/></param>
    /// <param name="replace">if a present binding should be replaced with the new binding</param>
    /// <param name="priority">the priority for this hook's execution. values closer to zero get executed sooner</param>
    /// <exception cref="ArgumentException">thrown if consumer already exists for key and replace is false</exception>
    Hook<T> Bind(string key, Action<T> eventConsumer, bool replace = false, Priority priority = Priority.Normal);

    /// <summary>
    /// Binds an event consumer to the hook under the given key, if a consumer already exists under the given key, and replace is false
    /// throws an exception
    /// </summary>
    /// <param name="key">key to bind consumer to</param>
    /// <param name="eventConsumer">a function taking in a <see cref="IHookEvent"/></param>
    /// <param name="replace">if a present binding should be replaced with the new binding</param>
    /// <param name="priority">the priority for this hook's execution. values closer to zero get executed sooner</param>
    /// <exception cref="ArgumentException">thrown if consumer already exists for key and replace is false</exception>
    Hook<T> Bind(Type key, Action<T> eventConsumer, bool replace = false, Priority priority = Priority.Normal) => Bind(key.Name, eventConsumer, replace, priority);


    /// <summary>
    /// Binds an event consumer to the hook under the given key, if a consumer already exists under the given key, and replace is false
    /// throws an exception
    /// </summary>
    /// <param name="key">key to bind consumer to</param>
    /// <param name="eventConsumer">a function taking in a <see cref="IHookEvent"/></param>
    /// <param name="replace">if a present binding should be replaced with the new binding</param>
    /// <param name="priority">the priority for this hook's execution. values closer to zero get executed sooner</param>
    /// <exception cref="ArgumentException">thrown if consumer already exists for key and replace is false</exception>
    Hook<T> Bind(string key, Action eventConsumer, bool replace = false, Priority priority = Priority.Normal)
    {
        return Bind(key, _ => eventConsumer(), replace, priority);
    }

    /// <summary>
    /// Binds an event consumer to the hook under the given key, if a consumer already exists under the given key, and replace is false
    /// throws an exception
    /// </summary>
    /// <param name="key">key to bind consumer to</param>
    /// <param name="eventConsumer">a function taking in a <see cref="IHookEvent"/></param>
    /// <param name="replace">if a present binding should be replaced with the new binding</param>
    /// <param name="priority">the priority for this hook's execution. values closer to zero get executed sooner</param>
    /// <exception cref="ArgumentException">thrown if consumer already exists for key and replace is false</exception>
    Hook<T> Bind(Type key, Action eventConsumer, bool replace = false, Priority priority = Priority.Normal)
    {
        return Bind(key, _ => eventConsumer(), replace, priority);
    }

    /// <summary>
    /// Propagates the incoming hook event to the subscribers of this hook
    /// </summary>
    /// <param name="hookEvent">event to propagate</param>
    void Propagate(T hookEvent);
}

// ReSharper disable once InconsistentNaming
public interface Hook
{
    /// <summary>
    /// Checks if a consumer is bound to the given key
    /// </summary>
    /// <param name="key">key of binding binding</param>
    /// <returns>true if consumer exists, otherwise false</returns>
    bool Exists(string key);

    /// <summary>
    /// Unbinds event consumer on given key, returns true if consumer was unbound otherwise returns false
    /// </summary>
    /// <param name="key">key to unbind consumer from</param>
    /// <returns>true if consumer was successfully unbound, otherwise false</returns>
    bool Unbind(string key);
}