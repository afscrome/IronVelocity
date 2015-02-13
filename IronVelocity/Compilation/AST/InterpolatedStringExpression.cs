using NVelocity.Exception;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.AST
{
    public class InterpolatedStringExpression : VelocityExpression
    {
        public IReadOnlyList<Expression> Parts { get; set; }

        public override Type Type { get { return typeof(string); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.InterpolatedString; } }


        public InterpolatedStringExpression(params Expression[] parts)
        {
            Parts = parts.ToArray();
        }

        private static MethodInfo _stringConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object[]) }, null);

        public override Expression Reduce()
        {
            if (Parts.Count == 1)
            {
                var element = Parts[0];
                return element.Type == typeof(string)
                    ? element
                    : Expression.Call(element, MethodHelpers.ToStringMethodInfo);
            }
            else
            {
                var objParts = Parts.Select(x => VelocityExpressions.ConvertIfNeeded(x, typeof(object)));
                return Expression.Call(_stringConcatMethodInfo, Expression.NewArrayInit(typeof(object),objParts));
            }
        }

        public InterpolatedStringExpression Update(params Expression[] parts)
        {
            return parts == Parts
                ? this
                : new InterpolatedStringExpression(parts);
        }

    }
}
