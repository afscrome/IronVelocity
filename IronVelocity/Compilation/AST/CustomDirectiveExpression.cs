using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public abstract class CustomDirectiveExpression : Directive
    {
        private readonly VelocityExpressionBuilder _builder;
        protected CustomDirectiveExpression(VelocityExpressionBuilder builder) 
        {
            _builder = builder;
        }

        protected CustomDirectiveExpression(ASTDirective node, VelocityExpressionBuilder builder)
            : base(node, builder)
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
