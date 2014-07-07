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
        public IReadOnlyCollection<Expression> Arguments { get; private set; }

        public ObjectArrayExpression(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTObjectArray))
                throw new ArgumentOutOfRangeException("node");

            Arguments = GetChildNodes(node)
                .Select(VelocityExpressionBuilder.Operand)
                .Select(x => VelocityExpressions.ConvertIfNeeded(x, typeof(object)))
                .ToList();
        }

        private ObjectArrayExpression(SymbolInformation symbols, IReadOnlyCollection<Expression> args)
        {
            Symbols = symbols;
            Arguments = args;
        }


        public override System.Linq.Expressions.Expression Reduce()
        {
            return Expression.New(MethodHelpers.ListConstructorInfo, Expression.NewArrayInit(typeof(object), Arguments));
        }


        public ObjectArrayExpression Update(IReadOnlyCollection<Expression> arguments)
        {
            if (arguments == Arguments)
                return this;

            return new ObjectArrayExpression(Symbols, arguments);
        }


        private static IEnumerable<INode> GetChildNodes(INode node)
        {
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                yield return node.GetChild(i);
            };
        }

    }
}
