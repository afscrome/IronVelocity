using IronVelocity.Binders;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IronVelocity.Compilation.AST
{
    public class BinaryMathematicalExpression : VelocityBinaryExpression
    {
        public MathematicalOperation Operation { get; private set; }
        public ExpressionType ExpressionType { get; private set; }

        public BinaryMathematicalExpression(INode node, MathematicalOperation op)
            : base(node)
        {
            Operation = op;
            ExpressionType = MathematicalOperationToExpressionType(op);
        }

        private BinaryMathematicalExpression(Expression left, Expression right, SymbolInformation symbols, MathematicalOperation op)
            : base(left, right, symbols)
        {
            Operation = op;
            ExpressionType = MathematicalOperationToExpressionType(op);
        }


        public override Expression Reduce()
        {
            var binder = new VelocityBinaryMathematicalOperationBinder(ExpressionType);

            return Expression.Dynamic(
                binder,
                binder.ReturnType,
                Left,
                Right
            );
        }

        public override Expression Update(Expression left, Expression right)
        {
            if (Left == left && Right == right)
                return this;
            else
                return new BinaryMathematicalExpression(left, right, Symbols, Operation);
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
    }
}
