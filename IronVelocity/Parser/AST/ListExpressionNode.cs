using System;
using System.Collections.Generic;


namespace IronVelocity.Parser.AST
{
    public class ListExpressionNode : ExpressionNode
    {
        public IReadOnlyList<ExpressionNode> Elements { get; private set; }

        public ListExpressionNode(ExpressionNode singleValue)
        {
            Elements = new[] { singleValue };
        }

        public ListExpressionNode(IReadOnlyList<ExpressionNode> elements)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");

            Elements = elements;
        }


        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitListExpressionNode(this);
        }

        public ListExpressionNode Update(IReadOnlyList<ExpressionNode> elements)
        {
            return elements == Elements
                ? this
                : new ListExpressionNode(elements);
        }
    }
}
