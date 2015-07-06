using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class UnaryExpressionNode : ExpressionNode
    {
        public UnaryOperation Operation { get; set; }
        public ExpressionNode Value { get; set; }

        public UnaryExpressionNode(UnaryOperation operation, ExpressionNode target)
        {
            if (target == null)
                throw new ArgumentNullException("value");

            Operation = operation;
            Value = target;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
}
