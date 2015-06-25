using IronVelocity.Compilation.Directives;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class VelocityExpressionBuilder
    {
        private readonly IDictionary<string, DirectiveExpressionBuilder> _directiveHandlers;
        public Stack<CustomDirectiveExpression> CustomDirectives { get; private set; }

        public IDictionary<string, DirectiveExpressionBuilder> DirectiveHandlers
        {
            get { return new Dictionary<string, DirectiveExpressionBuilder>(_directiveHandlers); }
        }


        public VelocityExpressionBuilder(IDictionary<string, DirectiveExpressionBuilder> directiveHandlers)
        {
            _directiveHandlers = directiveHandlers ?? new Dictionary<string, DirectiveExpressionBuilder>();
            CustomDirectives = new Stack<CustomDirectiveExpression>();
        }

        public void RegisterMacro(string name, LambdaExpression macro)
        {
            _directiveHandlers.Add(name, new MacroExecutionExpressionBuilder(name, macro));
        }

    }
}
