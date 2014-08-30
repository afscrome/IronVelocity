using IronVelocity.Reflection;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Binders
{
    [Obsolete]
    public static class ReflectionHelper
    {

        private static IArgumentConverter _conversionHelper = new ArgumentConverter();
        private static IMethodResolver _methodInvocationResolver = new MethodResolver(_conversionHelper);
        private static IMemberResolver _memberResolver = new MemberResolver();




        public static Expression MemberExpression(string name, DynamicMetaObject target)
        {
            return _memberResolver.MemberExpression(name, target);
        }


        public static Expression MemberExpression(string name, Type type, Expression expression)
        {
            return _memberResolver.MemberExpression(name, type, expression);
        }


        public static MethodInfo ResolveMethod(Type type, string name, params Type[] argTypes)
        {
            return _methodInvocationResolver.ResolveMethod(type, name, argTypes);
        }




        [Obsolete]
        public static bool CanBeImplicitlyConverted(Type from, Type to)
        {
            return _conversionHelper.CanBeConverted(from, to);
        }


        public static Expression ConvertMethodParameters(MethodInfo method, Expression target, DynamicMetaObject[] args)//, Type[] argTypeArray)
        {
            return _methodInvocationResolver.ConvertMethodParameters(method, target, args);
        }

        public static bool IsNullableType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return !(type.IsValueType && Nullable.GetUnderlyingType(type) == null);
        }

    }



}
