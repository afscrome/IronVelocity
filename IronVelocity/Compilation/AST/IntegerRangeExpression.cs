using NVelocity.Runtime.Parser.Node;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class IntegerRangeExpression : BinaryExpression
    {
        public IntegerRangeExpression(INode node)
            : base(node)
        {
        }

        public override Expression Reduce()
        {
            return Expression.Call(null, MethodHelpers.IntegerRangeMethodInfo,
                    VelocityExpressions.ConvertIfNeeded(Left, typeof(object)),
                    VelocityExpressions.ConvertIfNeeded(Right, typeof(object))
                    );
        }
    }
}
