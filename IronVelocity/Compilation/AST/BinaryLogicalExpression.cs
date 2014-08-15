using IronVelocity.Binders;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IronVelocity.Compilation.AST
{
    public class BinaryLogicalExpression : VelocityBinaryExpression
    {
        public LogicalOperation Operation { get; private set; }
        public override Type Type { get { return typeof(bool); } }


        internal BinaryLogicalExpression(Expression left, Expression right, SymbolInformation symbols, LogicalOperation op)
            : base(left, right, symbols)
        {
            Operation = op;
        }


        public override Expression Reduce()
        {
            var binder = new VelocityBinaryLogicalOperationBinder(Operation);

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
                return new BinaryLogicalExpression(left, right, Symbols, Operation);
        }
 

    }


}
