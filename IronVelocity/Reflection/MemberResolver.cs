using IronVelocity.Compilation;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class MemberResolver : IMemberResolver
    {
        private const BindingFlags _caseSensitiveBindingFlags = BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags _caseInsensitiveBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public MemberInfo GetMember(string name, Type type, bool caseSensitive)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var flags = caseSensitive
                ? _caseSensitiveBindingFlags
                : _caseInsensitiveBindingFlags;

            var property = type.GetProperty(name, flags);

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

        public Expression MemberExpression(string name, DynamicMetaObject target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

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

            return MemberExpression(name, target.LimitType, target.Expression);
        }


        public Expression MemberExpression(string name, Type type, Expression expression)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (name == null)
                throw new ArgumentNullException("name");

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                if (name.Equals("to_quote", StringComparison.OrdinalIgnoreCase))
                    return VelocityStrings.EscapeDoubleQuote(expression);
                else if (name.Equals("to_squote", StringComparison.OrdinalIgnoreCase))
                    return VelocityStrings.EscapeSingleQuote(expression);
            }

            MemberInfo member = null;
            try
            {
                member = GetMember(name, type, false);
            }
            catch (AmbiguousMatchException)
            {
                try
                {
                    member = GetMember(name, type, true);
                }
                catch (AmbiguousMatchException)
                {
                    Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "Ambiguous match for member '{0}' on type '{1}'", name, type.AssemblyQualifiedName), "Velocity");
                }
            }


            if (member == null)
            {
                //If no matching property or field, fall back to indexer with string param
                var indexer = type.GetProperty("Item", _caseSensitiveBindingFlags, null, null, new[] { typeof(string) }, null) as PropertyInfo;
                if (indexer != null)
                {
                    return Expression.MakeIndex(
                        //VelocityExpressions.ConvertReturnTypeIfNeeded(target, indexer),
                            VelocityExpressions.ConvertIfNeeded(expression, indexer.DeclaringType),
                            indexer,
                            new[] { Expression.Constant(name) }
                        );
                }
                else
                {
                    var method = IronVelocity.Binders.ReflectionHelper.ResolveMethod(type, name);
                    if (method != null)
                    {
                        return Expression.Call(VelocityExpressions.ConvertIfNeeded(expression, method.DeclaringType), method);
                    }
                }
                
                if (type.IsArray && name.Equals("count", StringComparison.OrdinalIgnoreCase))
                {
                    return MemberExpression("Length", type, expression);
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
            Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "Unable to resolve Property '{0}' on type '{1}'", name, type.AssemblyQualifiedName), "Velocity");
            return null;
        }

    }

}
