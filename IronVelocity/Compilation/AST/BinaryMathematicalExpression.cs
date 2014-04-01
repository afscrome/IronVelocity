using IronVelocity.Binders;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class BinaryMathematicalExpression : BinaryExpression
    {
        public BinaryMathematicalExpression(INode node, MathematicalOperation op)
            :base(node)
        {
            Operation = op;
            ExpressionType = MathematicalOperationToExpressionType(op);
        }

        public MathematicalOperation Operation { get; private set; }
        public ExpressionType ExpressionType { get; private set; }

        protected override System.Linq.Expressions.Expression ReduceInternal()
        {
            return Expression.Dynamic(
                new VelocityBinaryMathematicalOperationBinder(ExpressionType),
                typeof(object),
                Left,
                Right
            );
        }

        public ExpressionType MathematicalOperationToExpressionType(MathematicalOperation op)
        {
            switch (op)
            {
                case MathematicalOperation.Add:
                    return ExpressionType.Add;
                case MathematicalOperation.Subtract:
                    return ExpressionType.Subtract;
                case MathematicalOperation.Multiply:
                    return ExpressionType.Multiply;
                case MathematicalOperation.Divide:
                    return ExpressionType.Divide;
                case MathematicalOperation.Modulo:
                    return ExpressionType.Modulo;
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }
    }

    public enum MathematicalOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo
    }
}
