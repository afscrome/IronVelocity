using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.Directives
{
    public class MacroDefinitionExpressionBuilder : DirectiveExpressionBuilder
    {

        public override string Name { get { return "macro"; } }

        public override Expression Build(ASTDirective node, NVelocityNodeToExpressionConverter converter)
        {
            if (node.ChildrenCount < 2)
                throw new ArgumentOutOfRangeException("node", "Macro Node needs at least 2 children");

            var firstChild = node.GetChild(0);

            if (!(firstChild is ASTWord))
                throw new ArgumentOutOfRangeException("node");

            var name = firstChild.Literal;

            var argCount = node.ChildrenCount - 2;

            var parameters = new ParameterExpression[argCount];

            for (int i = 0; i < argCount; i++)
            {
                var reference = node.GetChild(i + 1) as ASTReference;
                if (reference == null)
                    throw new NotSupportedException();

                parameters[i] = Expression.Parameter(typeof(object), reference.Literal.TrimStart('$'));
            }

            var bodyNode = node.GetChild(node.ChildrenCount -1);
            var body = new RenderedBlock(converter.GetBlockExpressions(bodyNode));

            var replacements = parameters.ToDictionary(x => x.Name, k => (Expression)k);
            var visitor = new VariableReplacementVisitor(replacements);

            var lambdaName = "Macro_" + name;
            var lambda = Expression.Lambda(Expression.Block(visitor.Visit(body)), lambdaName, parameters);

            converter.Builder.RegisterMacro(name, lambda);

            return Constants.EmptyExpression;
        }


        private class VariableReplacementVisitor : VelocityExpressionVisitor
        {
            private readonly IDictionary<string, Expression> _variableReplacements;

            public VariableReplacementVisitor(IDictionary<string, Expression> variableReplacements)
            {
                if (variableReplacements == null)
                    throw new ArgumentNullException("variableReplacements");

                _variableReplacements = variableReplacements;
            }

            protected override Expression VisitVariable(VariableExpression node)
            {
                Expression value;

                return _variableReplacements.TryGetValue(node.Name, out value)
                    ? Visit(value)
                    : base.VisitVariable(node); ;
            }
        }
    }

}
