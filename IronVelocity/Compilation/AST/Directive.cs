using IronVelocity.Compilation.Directives;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class Directive : VelocityExpression
    {
        public string Name { get; private set; }
        public ASTDirective Node { get; private set; }
        private readonly IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers;
        private readonly VelocityExpressionBuilder _builder;

        public Directive(INode node, IDictionary<Type, DirectiveExpressionBuilder> handlers, VelocityExpressionBuilder builder)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var directive = node as ASTDirective;
            if (directive == null)
                throw new ArgumentOutOfRangeException("node");

            if (handlers == null)
                throw new ArgumentNullException("handlers");

            if (builder == null)
                throw new ArgumentNullException("builder");

            Name = directive.DirectiveName;
            Node = directive;
            _directiveHandlers = handlers;
            _builder = builder;
        }

        protected override Expression ReduceInternal()
        {
            if (Node.Directive == null)
                return Expression.Constant(Node.Literal);

            DirectiveExpressionBuilder builder;
            if (_directiveHandlers.TryGetValue(Node.Directive.GetType(), out builder))
                return builder.Build(Node, _builder);
            else
                throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "Unable to handle directive type '{0}'", Node.DirectiveName));
        }

        public override Type Type { get { return typeof(void); } }
    }

}
