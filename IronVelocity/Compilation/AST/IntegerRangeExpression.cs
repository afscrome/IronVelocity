using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class IntegerRangeExpression : VelocityBinaryExpression
    {
        public override Type Type { get { return typeof(IList<int>); } }

        public IntegerRangeExpression(Expression left, Expression right, SymbolInformation symbols)
            : base(left, right, symbols)
        {            
        }


        public override Expression Reduce()
        {
            return Expression.Call(null, MethodHelpers.IntegerRangeMethodInfo,
                    VelocityExpressions.ConvertIfNeeded(Left, typeof(object)),
                    VelocityExpressions.ConvertIfNeeded(Right, typeof(object))
                    );
        }

        public override VelocityBinaryExpression Update(Expression left, Expression right)
        {
            if (Left == left && Right == right)
                return this;
            else
                return new IntegerRangeExpression(left, right, Symbols);
        }


    }
}
