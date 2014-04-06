using IronVelocity.Binders;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

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

        protected override Expression ReduceInternal()
        {
            var binder = new VelocityBinaryOperationBinder(ExpressionType);
            return Expression.Dynamic(
                binder,
                binder.ReturnType,
                Left,
                Right
            );
        }

        public static ExpressionType MathematicalOperationToExpressionType(MathematicalOperation op)
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
                case MathematicalOperation.And:
                    return ExpressionType.And;
                case MathematicalOperation.Or:
                    return ExpressionType.Or;
                case MathematicalOperation.Equal:
                    return ExpressionType.Equal;
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
        Modulo,
        And,
        Or,
        Equal
    }
}
