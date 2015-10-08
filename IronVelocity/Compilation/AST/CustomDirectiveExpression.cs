using NVelocity.Runtime.Parser.Node;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract class CustomDirectiveExpression : Directive
    {
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.CustomDirective; } }

        protected CustomDirectiveExpression()
        {
        }

        public virtual Expression ProcessChildDirective(string name, INode node)
        {
            return null;
        }

        public override Expression Reduce()
        {
            return ReduceInternal();
        }

        protected abstract Expression ReduceInternal();

    }
}
