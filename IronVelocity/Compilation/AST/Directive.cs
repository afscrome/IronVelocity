using IronVelocity.Compilation.Directives;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract class Directive : VelocityExpression
    {
        public string Name { get; private set; }
        public ASTDirective Node { get; private set; }
        private readonly VelocityExpressionBuilder _builder;

        protected Directive(INode node, VelocityExpressionBuilder builder)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var directive = node as ASTDirective;
            if (directive == null)
                throw new ArgumentOutOfRangeException("node");

            if (builder == null)
                throw new ArgumentNullException("builder");

            Name = directive.DirectiveName;
            Node = directive;
            _builder = builder;
        }

        public override Type Type { get { return typeof(void); } }
    }

}
