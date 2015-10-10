using IronVelocity.Compilation;
using IronVelocity.Compilation.AST;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class MemberResolver : IMemberResolver
    {
        private const BindingFlags _caseSensitiveBindingFlags = BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags _caseInsensitiveBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public MemberInfo GetMember(string name, Type type, bool caseSensitive, MemberAccessMode mode)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var flags = caseSensitive
                ? _caseSensitiveBindingFlags
                : _caseInsensitiveBindingFlags;

            var property = type.GetProperty(name, flags);

            if (property != null)
            {
                if ((mode == MemberAccessMode.Read && property.CanRead && property.GetMethod.IsPublic)
                    || (mode == MemberAccessMode.Write && property.CanWrite && property.SetMethod.IsPublic))
                {
                    if (caseSensitive)
                        return property;
                }
                else {
                    property = null;
                }
            }

            if (caseSensitive && property != null)
                return property;

            var field = type.GetField(name, flags);

            if (property != null && field != null)
                throw new AmbiguousMatchException();

            if (property != null)
                return property;
            else if (field != null)
                return field;

            return null;

        }

        public Expression MemberExpression(string name, DynamicMetaObject target, MemberAccessMode mode)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            // Also if the value is typeof(ENUM), then return the relevant enumerated type
            if (target.Value is Type)
            {
                var valueType = (Type)target.Value;
                if (valueType.IsEnum)
                {
                    try
                    {
                        return Expression.Constant(Enum.Parse(valueType, name, true), valueType);
                    }
                    catch (ArgumentException) { }
                }
            }

            return MemberExpression(name, target.LimitType, target.Expression, mode);
        }


        public Expression MemberExpression(string name, Type type, Expression expression, MemberAccessMode mode)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                if (name.Equals("to_quote", StringComparison.OrdinalIgnoreCase))
                    return VelocityStrings.EscapeDoubleQuote(expression);
                else if (name.Equals("to_squote", StringComparison.OrdinalIgnoreCase))
                    return VelocityStrings.EscapeSingleQuote(expression);
            }


            if (typeof(Type).IsAssignableFrom(type))
            {
                var globalExpression = expression as GlobalVariableExpression;
                if (globalExpression != null)
                {
                    var valueType = globalExpression.Value as Type;
                    if (valueType != null && valueType.IsEnum)
                    {
                        var result = Enum.Parse(valueType, name);
                        if (result != null)
                        {
                            return Expression.Constant(result, valueType);
                        }
                    }
                }
            }

            MemberInfo member = null;
            try
            {
                member = GetMember(name, type, false, mode);
            }
            catch (AmbiguousMatchException)
            {
                member = GetMember(name, type, true, mode);
            }


            if (member == null)
            {
                //If no matching property or field, fall back to indexer with string param
                var indexer = type.GetProperty("Item", _caseSensitiveBindingFlags, null, null, new[] { typeof(string) }, null) as PropertyInfo;
                if (indexer != null)
                {
                    return Expression.MakeIndex(
                            VelocityExpressions.ConvertIfNeeded(expression, indexer.DeclaringType),
                            indexer,
                            new[] { Expression.Constant(name) }
                        );
                }
                else if (mode == MemberAccessMode.Read)
                {
                    var method = IronVelocity.Binders.ReflectionHelper.ResolveMethod(type, name);
                    if (method != null)
                    {
                        return Expression.Call(VelocityExpressions.ConvertIfNeeded(expression, method.DeclaringType), method);
                    }
                }
                
                if (type.IsArray && name.Equals("count", StringComparison.OrdinalIgnoreCase))
                {
                    return MemberExpression("Length", type, expression, mode);
                }
            }
            else
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    return Expression.Property(
                        //VelocityExpressions.ConvertReturnTypeIfNeeded(target, member),
                            VelocityExpressions.ConvertIfNeeded(expression, property.DeclaringType),
                            property
                        );
                }
                else
                {
                    var field = member as FieldInfo;
                    if (field != null)
                    {
                        return Expression.Field(
                            //VelocityExpressions.ConvertReturnTypeIfNeeded(target, member),
                                VelocityExpressions.ConvertIfNeeded(expression, field.DeclaringType),
                                field
                            );
                    }

                }
            }

            return null;
        }

    }

}
