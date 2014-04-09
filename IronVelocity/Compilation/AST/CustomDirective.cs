using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public abstract class CustomDirective : Directive
    {

        protected CustomDirective(ASTDirective node, VelocityExpressionBuilder builder)
            : base(node, builder)
        {
        }

        public abstract Expression ProcessChildDirective(string name, INode node);

        public override Expression Reduce()
        {
            Builder.CustomDirectives.Push(this);
            try
            {
                return ReduceInternal();
            }
            finally
            {
                Builder.CustomDirectives.Pop();
            }
        }

        protected abstract Expression ReduceInternal();

    }
}
