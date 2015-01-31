using NVelocity.Runtime.Parser.Node;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract class CustomDirectiveExpression : Directive
    {
        private readonly VelocityExpressionBuilder _builder;
        protected CustomDirectiveExpression(VelocityExpressionBuilder builder) 
        {
            _builder = builder;
        }

        public virtual Expression ProcessChildDirective(string name, INode node)
        {
            return null;
        }

        public override Expression Reduce()
        {
            _builder.CustomDirectives.Push(this);
            try
            {
                return ReduceInternal();
            }
            finally
            {
                _builder.CustomDirectives.Pop();
            }
        }

        protected abstract Expression ReduceInternal();

    }
}
