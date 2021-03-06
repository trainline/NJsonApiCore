﻿using System;
using System.Linq.Expressions;
using NJsonApi.Utils;


namespace NJsonApi.Infrastructure
{
    public class PropertyHandle<TResource, TProperty> : IPropertyHandle<TResource, TProperty>
    {
        public PropertyHandle(Expression<Func<TResource, TProperty>> expression)
        {
            Type = typeof(TProperty);
            var pi = expression.GetPropertyInfo();
            Getter = pi.ToCompiledGetterFunc<TResource, TProperty>();
            Setter = pi.ToCompiledSetterAction<object, TProperty>();
            Name = pi.Name;
            Expression = expression;
        }

        public Type Type { get; private set; }
        public Expression<Func<TResource, TProperty>> Expression { get; private set; }
        public Func<TResource, TProperty> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }
        public string Name { get; private set; }

        public Delegate GetterDelegate { get { return Getter; } }
        public Delegate SetterDelegate { get { return Setter; } }
    }
}
