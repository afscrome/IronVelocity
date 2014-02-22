using IronVelocity.Compilation;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Binders
{
    public static class ReflectionHelper
    {
        private const BindingFlags _caseSensitiveBindingFlags = BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags _caseInsensitiveBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        private static MemberInfo GetMember(string name, Type type, bool caseSensitive)
        {
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
            else
                return field;

        }

        public static Expression MemberExpression(string name, DynamicMetaObject target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            MemberInfo member = null;
            try
            {
                member = ReflectionHelper.GetMember(name, target.LimitType, false);
            }
            catch (AmbiguousMatchException)
            {
                try
                {
                    member = ReflectionHelper.GetMember(name, target.LimitType, true);
                }
                catch (AmbiguousMatchException)
                {
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Ambiguous match for member '{0}' on type '{1}'", name, target.LimitType.AssemblyQualifiedName), "Velocity");
                }
            }


            if (member == null)
            {
                //If no matching property or field, fall back to indexer with string param
                var indexer = target.LimitType.GetProperty("Item", _caseSensitiveBindingFlags, null, null, new[] { typeof(string) }, null);
                if (indexer == null)
                {
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to resolve Property '{0}' on type '{1}'", name, target.LimitType.AssemblyQualifiedName), "Velocity");
                    return null;
                }
                else
                {
                    return Expression.MakeIndex(
                            VelocityExpressions.ConvertReturnTypeIfNeeded(target, indexer),
                            (PropertyInfo)indexer,
                            new[] { Expression.Constant(name) }
                        );
                }
            }
            else
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    return Expression.Property(
                            VelocityExpressions.ConvertReturnTypeIfNeeded(target, member),
                            property
                        );
                }
                else
                {
                    var field = member as FieldInfo;
                    if (field != null)
                    {
                        return Expression.Field(
                                VelocityExpressions.ConvertReturnTypeIfNeeded(target, member),
                                field
                            );
                    }
                }
            }
            return null;
        }

    }
}
