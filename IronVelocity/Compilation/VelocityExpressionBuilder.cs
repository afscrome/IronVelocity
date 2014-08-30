using IronVelocity.Compilation.Directives;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IronVelocity.Compilation.AST
{
    public class VelocityExpressionBuilder
    {

        private readonly IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers;
        public ParameterExpression OutputParameter { get; set; }
        public Stack<CustomDirectiveExpression> CustomDirectives { get; private set; }

        public IDictionary<Type, DirectiveExpressionBuilder> DirectiveHandlers
        {
            get { return new Dictionary<Type, DirectiveExpressionBuilder>(_directiveHandlers); }
        }


        public VelocityExpressionBuilder(IDictionary<Type, DirectiveExpressionBuilder> directiveHandlers)
            : this (directiveHandlers, "$output")
        {
        }

        public VelocityExpressionBuilder(IDictionary<Type, DirectiveExpressionBuilder> directiveHandlers, string parameterName)
        {
            _directiveHandlers = directiveHandlers ?? new Dictionary<Type, DirectiveExpressionBuilder>();
            OutputParameter = Expression.Parameter(typeof(StringBuilder), parameterName);
            CustomDirectives = new Stack<CustomDirectiveExpression>();
        }

        public IReadOnlyCollection<Expression> GetBlockExpressions(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            //ASTprocess is a special case for the root, otherwise it behaves exactly like ASTBlock
            if (!(node is ASTBlock || node is ASTprocess))
                throw new ArgumentOutOfRangeException("node");

            var expressions = new List<Expression>(node.ChildrenCount);

            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
                Expression expr;
                switch (child.Type)
                {
                    case ParserTreeConstants.TEXT:
                    case ParserTreeConstants.ESCAPE:
                        var content = NodeUtils.tokenLiteral(child.FirstToken);
                        expr = Expression.Constant(content);
                        break;
                    case ParserTreeConstants.ESCAPED_DIRECTIVE:
                        expr = Expression.Constant(child.Literal);
                        break;
                    case ParserTreeConstants.REFERENCE:
                        expr = VelocityExpression.Reference(child);
                        break;
                    case ParserTreeConstants.IF_STATEMENT:
                        expr = VelocityExpression.IfDirective(child, this);
                        break;
                    case ParserTreeConstants.SET_DIRECTIVE:
                        expr = SetDirective(child);
                        break;
                    case ParserTreeConstants.DIRECTIVE:
                        expr = Directive(child);
                        break;
                    case ParserTreeConstants.COMMENT:
                        continue;

                    default:
                        throw new NotSupportedException("Node type not supported in a block: " + child.GetType().Name);
                }

                expressions.Add(expr);
            }

            return expressions;
        }

        public Expression Directive(INode node)
        {
            var directiveNode = (ASTDirective)node;

            if (directiveNode == null)
                throw new ArgumentOutOfRangeException("node");

            if (directiveNode.DirectiveName == "macro")
                throw new NotSupportedException("TODO: #macro support");
            if (directiveNode.DirectiveName == "include")
                throw new NotSupportedException("TODO: #include support");
            if (directiveNode.DirectiveName == "parse")
                throw new NotSupportedException("TODO: #parse support");

            DirectiveExpressionBuilder builder;
            foreach (var customDirective in CustomDirectives)
            {
                var expr = customDirective.ProcessChildDirective(directiveNode.DirectiveName, directiveNode);
                if (expr != null)
                    return expr;
            }

            if (directiveNode.Directive != null && _directiveHandlers.TryGetValue(directiveNode.Directive.GetType(), out builder))
            {
                return builder.Build(directiveNode, this);
            }
            else
                return new UnrecognisedDirective(directiveNode, this);
        }
        private static Expression SetDirective(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTSetDirective))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException("node", "Expected only one child");

            return VelocityExpression.Expr(node.GetChild(0));
        }
    }
}
