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
        public string Value { get; private set; }
        public IReadOnlyList<Expression> Parts { get; set; }
        //public override System.Type Type { get { return typeof(string); } }


        public InterpolatedStringExpression(string value)
        {
            Value = value;
            //TODO; Refactor to share with VelocityExpressionTreeBuilder, or reuse the same parser
            var parser = new NVelocity.Runtime.RuntimeInstance().CreateNewParser();
            using (var reader = new System.IO.StringReader(Value))
            {
                SimpleNode ast;
                try
                {
                    ast = parser.Parse(reader, null);
                }
                catch (ParseErrorException)
                {
                    ast = null;
                }

                //If we fail to parse, the ast returned will be null, so just return our normal string
                if (ast == null)
                    Parts = new Expression[] { Expression.Constant(Value) };
                else
                    Parts = new VelocityExpressionBuilder(null).GetBlockExpressions(ast)
                    .Where(x => x.Type != typeof(void))
                    .ToArray();

            }
        }

        private InterpolatedStringExpression(string value, IReadOnlyList<Expression> parts)
        {
            Value = value;
            Parts = parts;
        }

        private static MethodInfo _stringConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object[]) }, null);

        public override Expression Reduce()
        {
            if (Parts.Count == 1)
                return Parts[0];
            else
            {
                var objParts = Parts.Select(x => VelocityExpressions.ConvertIfNeeded(x, typeof(object)));
                return Expression.Call(_stringConcatMethodInfo, Expression.NewArrayInit(typeof(object),objParts));
            }
        }

        public Expression Update(IReadOnlyList<Expression> parts)
        {
            return parts == Parts
                ? this
                : new InterpolatedStringExpression(Value, parts);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            bool changed = false;

            var visitedValues = new Expression[Parts.Count];
            for (int i = 0; i < visitedValues.Length; i++)
            {
                visitedValues[i] = visitor.Visit(Parts[i]);
                if (visitedValues[i] != Parts[i])
                {
                    changed = true;
                }
            }

            if (changed)
                return Update(visitedValues);
            else
                return this;
        }

    }
}
