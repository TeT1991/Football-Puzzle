using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static Dictionary<Type, IService> _services = new();

    public static void Register<T>(T service) where T : IService
    {
        Type type = typeof(T);

        if (_services.ContainsKey(type))
        {
            throw new InvalidOperationException("Service Already EXIST");
        }

        _services[type] = service;
    }

    public static T Get<T>() where T : IService
    {
        if (_services.ContainsKey(typeof(T)))
        {
            return (T)_services[typeof(T)];
        }

        throw new InvalidOperationException("Service not EXIST");
    }
}