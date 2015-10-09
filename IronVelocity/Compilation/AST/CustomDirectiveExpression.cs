using NVelocity.Runtime.Parser.Node;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract class CustomDirectiveExpression : Directive
    {
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.CustomDirective;

        protected CustomDirectiveExpression()
        {
        }

        public virtual Expression ProcessChildDirective(string name, INode node) => null;
        public override Expression Reduce() => ReduceInternal();
        protected abstract Expression ReduceInternal();
    }
}
