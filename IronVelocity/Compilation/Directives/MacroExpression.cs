using IronVelocity.Compilation.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    /*
    public class MacroExpression : CustomDirectiveExpression
    {
        public IReadOnlyList<ParameterExpression> Parameters { get; private set; }
        public Expression Body { get; private set; }


        public MacroExpression(IReadOnlyList<ParameterExpression> parameters, Expression body, VelocityExpressionBuilder builder)
            : base(builder)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (body == null)
                throw new ArgumentNullException("body");

            Parameters = parameters;
            Body = body;
        }

        protected override Expression ReduceInternal()
        {
            return Expression.Block(Type, CreateLambda());
        }


    }*/
}
