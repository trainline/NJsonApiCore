using System;
using System.Linq.Expressions;

namespace NJsonApi.Infrastructure
{
    public interface IPropertyHandle
    {
        Type Type { get; }
        Delegate GetterDelegate { get; }
        Delegate SetterDelegate { get; }
        string Name { get; }
    }

    public interface IPropertyHandle<TResource, TProperty> : IPropertyHandle
    {
        Expression<Func<TResource, TProperty>> Expression { get; }
        Func<TResource, TProperty> Getter { get; }
        Action<object, object> Setter { get; }
    }
}
