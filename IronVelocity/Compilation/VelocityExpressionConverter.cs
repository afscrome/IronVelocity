using IronVelocity.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Parser.AST;
using IronVelocity.Compilation.AST;

namespace IronVelocity.Compilation
{
    public class VelocityExpressionConverter : IAstVisitor<Expression>
    {
        private readonly SourceInfo _tempSourceInfo = new SourceInfo(0, 0, 0, 0);

        public Expression Visit(SyntaxNode node)
        {
            return node.Accept(this);
        }

        public Expression VisitArgumentsNode(ArgumentsNode node)
        {
            throw new NotImplementedException();
        }

        public Expression VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            throw new NotImplementedException();
        }

        public Expression VisitBooleanLiteralNode(BooleanLiteralNode node)
        {
            return Expression.Constant(node.Value);
        }

        public Expression VisitFloatingPointLiteralNode(FloatingPointLiteralNode node)
        {
            return Expression.Constant(node.Value);
        }

        public Expression VisitIntegerLiteralNode(IntegerLiteralNode node)
        {
            return Expression.Constant(node.Value);
        }

        public Expression VisitListExpressionNode(ListExpressionNode node)
        {
            var visitedElements = VisitChildren(node.Elements);
            return new ObjectArrayExpression(_tempSourceInfo, visitedElements);
        }


        public Expression VisitMethod(Method node)
        {
            var target = Visit(node.Target);
            return new MethodInvocationExpression(target, node.Name, null, _tempSourceInfo);
        }


        public Expression VisitProperty(Property node)
        {
            var target = Visit(node.Target);

            return new PropertyAccessExpression(target, node.Name, _tempSourceInfo);
        }

        public Expression VisitReferenceNode(ReferenceNode node)
        {
            //TODO: this doesn't handle null fallback
            return Visit(node.Value);
        }

        public Expression VisitRenderedOutputNode(RenderedOutputNode node)
        {
            var visitedChildren = VisitChildren(node.Children);
            return new RenderedBlock(visitedChildren);
        }

        public Expression VisitStringNode(StringNode node)
        {
            if (node.IsInterpolated)
                throw new NotSupportedException();

            return Expression.Constant(node.Value);
        }

        public Expression VisitTextNode(TextNode node)
        {
            return Expression.Constant(node.Content);
        }

        public Expression VisitUnaryExpression(UnaryExpressionNode node)
        {
            var value = Visit(node.Value);
            switch (node.Operation)
            {
                case UnaryOperation.Not:
                    value = VelocityExpressions.CoerceToBoolean(value);
                    return Expression.Not(value);
                case UnaryOperation.Parenthesised:
                    return value;
                default:
                    throw new ArgumentOutOfRangeException("node");
            }
        }



        public Expression VisitVariable(Variable node)
        {
            return new VariableExpression(node.Name);
        }

        public Expression VisitWordNode(WordNode node)
        {
            throw new NotImplementedException();
        }


        private IReadOnlyList<Expression> VisitChildren(IReadOnlyList<SyntaxNode> children)
        {
            var childrenCount = children.Count;
            var newChildren = new Expression[childrenCount];

            for (int i = 0; i < childrenCount; i++)
            {
                newChildren[i] = Visit(children[i]);
            }

            return newChildren;
        }
    }
}
