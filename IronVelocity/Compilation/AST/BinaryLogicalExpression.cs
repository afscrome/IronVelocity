using IronVelocity.Binders;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IronVelocity.Compilation.AST
{
    public class BinaryLogicalExpression : VelocityBinaryExpression
    {
        public BinaryLogicalExpression(INode node, LogicalOperation op)
            :base(node)
        {
            Operation = op;
        }

        public LogicalOperation Operation { get; private set; }

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

        public override Type Type
        {
            get
            {
                return typeof(bool);
            }
        }
    }


}
