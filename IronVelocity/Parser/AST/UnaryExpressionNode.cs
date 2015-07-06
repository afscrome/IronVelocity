using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class UnaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Value { get; set; }
        public UnaryOperation Operation { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
}
