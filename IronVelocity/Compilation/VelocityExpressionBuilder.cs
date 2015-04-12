using IronVelocity.Compilation.Directives;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IronVelocity.Compilation.AST
{
    public class VelocityExpressionBuilder
    {
        private readonly IDictionary<string, DirectiveExpressionBuilder> _directiveHandlers;
        public ParameterExpression OutputParameter { get; set; }
        public Stack<CustomDirectiveExpression> CustomDirectives { get; private set; }

        public IDictionary<string, DirectiveExpressionBuilder> DirectiveHandlers
        {
            get { return new Dictionary<string, DirectiveExpressionBuilder>(_directiveHandlers); }
        }


        public VelocityExpressionBuilder(IDictionary<string, DirectiveExpressionBuilder> directiveHandlers)
            : this (directiveHandlers, "$output")
        {
        }

        public VelocityExpressionBuilder(IDictionary<string, DirectiveExpressionBuilder> directiveHandlers, string parameterName)
        {
            _directiveHandlers = directiveHandlers ?? new Dictionary<string, DirectiveExpressionBuilder>();
            OutputParameter = Expression.Parameter(typeof(StringBuilder), parameterName);
            CustomDirectives = new Stack<CustomDirectiveExpression>();
        }

        public void RegisterMacro(string name, LambdaExpression macro)
        {
            _directiveHandlers.Add(name, new MacroExecutionExpressionBuilder(name, macro));
        }

    }
}
