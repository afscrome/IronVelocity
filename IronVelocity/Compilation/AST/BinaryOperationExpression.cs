using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class BinaryOperationExpression : VelocityBinaryExpression
    {
        private readonly BinaryOperationBinder _binder;
        public ExpressionType ExpressionType => _binder.Operation;

        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.Binary;

        public BinaryOperationExpression(Expression left, Expression right, SourceInfo sourceInfo, BinaryOperationBinder binder)
            : base(left, right, sourceInfo)
        {
            _binder = binder;
        }

        public override Expression Reduce()
        {
            return Expression.Dynamic(
                _binder,
                _binder.ReturnType,
                Left,
                Right
            );
        }

        public override VelocityBinaryExpression Update(Expression left, Expression right)
        {
            if (Left == left && Right == right)
                return this;
            else
                return new BinaryOperationExpression(left, right, SourceInfo, _binder);
        }
    }
}
