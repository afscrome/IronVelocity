using IronVelocity.Compilation.AST;
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

        public static Expression MemberExpression(string name, DynamicMetaObject target, MemberAccessMode mode)
            => _memberResolver.MemberExpression(name, target, mode);

        public static Expression MemberExpression(string name, Type type, Expression expression, MemberAccessMode mode)
            => _memberResolver.MemberExpression(name, type, expression, mode);

        public static MethodInfo ResolveMethod(Type type, string name, params Type[] argTypes)
            => _methodInvocationResolver.ResolveMethod(type.GetTypeInfo(), name, argTypes);


        public static bool IsConstantType(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression is DynamicExpression || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(expression.Type))
                return false;

            if (expression is ConstantExpression || expression is GlobalVariableExpression)
                return true;

            //Interpolated & dictionary strings will always return the same type
            if (expression is InterpolatedStringExpression || expression is DictionaryStringExpression
                || expression is DictionaryExpression || expression is ObjectArrayExpression)
                return true;

            if (expression.Type == typeof(void))
                return true;

            //If the return type is sealed, we can't get any subclasses back
            if (expression.Type.IsSealed)
                return true;

            return false;
        }

        public static bool CanBeImplicitlyConverted(Type from, Type to)
            => _conversionHelper.CanBeConverted(from, to);

        public static Expression ConvertMethodParameters(MethodInfo method, Expression target, DynamicMetaObject[] args)//, Type[] argTypeArray)
            => _methodInvocationResolver.ConvertMethodParameters(method, target, args);


        public static bool IsNullableType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return !(type.IsValueType && Nullable.GetUnderlyingType(type) == null);
        }

    }



}
