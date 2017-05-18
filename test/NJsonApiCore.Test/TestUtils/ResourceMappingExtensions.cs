using NJsonApi.Utils;
using System;
using System.Linq.Expressions;

namespace NJsonApi
{
    public static class ResourceMappingExtensions
    {
        public static void AddPropertyGetter<TEntity, TController>(this ResourceMapping<TEntity, TController> resourceMapping, string key, Expression<Func<TEntity, object>> expression)
        {
            resourceMapping.PropertyGetters.Add(key, ExpressionUtils.CompileToObjectTypedFunction(expression));
        }

        public static void AddPropertySetter<TEntity, TController>(this ResourceMapping<TEntity, TController> resourceMapping, string key, Expression<Action<TEntity, object>> expression)
        {
            var convertedExpression = ExpressionUtils.ConvertToObjectTypeExpression(expression);

            resourceMapping.PropertySettersExpressions.Add(key, convertedExpression);
            resourceMapping.PropertySetters.Add(key, convertedExpression.Compile());
        }
    }
}
