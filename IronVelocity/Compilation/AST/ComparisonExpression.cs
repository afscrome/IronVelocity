using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class ComparisonExpression : VelocityBinaryExpression
    {
        public ComparisonOperation Operation { get; private set; }
        public override Type Type { get { return typeof(bool); } }


        internal ComparisonExpression(Expression left, Expression right, SymbolInformation symbols, ComparisonOperation op)
            : base(left, right, symbols)
        {
            Operation = op;
        }


        public override Expression Reduce()
        {
            var binder = BinderHelper.Instance.GetComparisonOperationBinder(Operation);

            return Expression.Dynamic(
                binder,
                binder.ReturnType,
                Left,
                Right
            );
        }

        public override VelocityBinaryExpression Update(Expression left, Expression right)
        {
            if (Left == left && Right == right)
                return this;
            else
                return new ComparisonExpression(left, right, Symbols, Operation);
        }
 

    }


}
