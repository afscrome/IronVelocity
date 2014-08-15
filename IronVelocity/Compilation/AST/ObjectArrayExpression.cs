using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class ObjectArrayExpression : VelocityExpression
    {
        public IReadOnlyList<Expression> Values { get; private set; }
        public override Type Type { get { return typeof(IList<object>); } }

        public ObjectArrayExpression(SymbolInformation symbols, IReadOnlyList<Expression> args)
        {
            Symbols = symbols;
            Values = args;
        }


        public override Expression Reduce()
        {
            var objValues = Values.Select(x => VelocityExpressions.ConvertIfNeeded(x, typeof(object)));
            return Expression.New(MethodHelpers.ListConstructorInfo, Expression.NewArrayInit(typeof(object), objValues));
        }


        public ObjectArrayExpression Update(IReadOnlyList<Expression> arguments)
        {
            if (arguments == Values)
                return this;

            return new ObjectArrayExpression(Symbols, arguments);
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            bool changed = false;

            var visitedValues = new Expression[Values.Count];
            for (int i = 0; i < visitedValues.Length; i++)
            {
                var value = visitor.Visit(Values[i]);
                if (value != Values[i])
                {
                    changed = true;
                }
                visitedValues[i] = value;
            }

            var result = changed ?
                Update(visitedValues)
                : this;

            //return result;
            return visitor.Visit(result.ReduceAndCheck());
        }

    }
}
