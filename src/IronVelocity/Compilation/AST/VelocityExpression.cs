using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract partial class VelocityExpression : Expression
    {
        public SourceInfo SourceInfo { get; protected set; }
        public override bool CanReduce => true;
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => typeof(object);

        protected VelocityExpression() { }


        public abstract override Expression Reduce();
        public abstract VelocityExpressionType VelocityExpressionType { get; }
    }

}
