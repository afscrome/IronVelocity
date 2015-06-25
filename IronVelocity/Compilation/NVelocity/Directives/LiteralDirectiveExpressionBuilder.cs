using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public class LiteralDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        public override string Name { get { return "literal"; } }
        public override Type NVelocityDirectiveType { get { return typeof(Literal); } }

        public override Expression Build(ASTDirective node, NVelocityNodeToExpressionConverter converter)
        {
            return new LiteralDirective(node, converter);
        }
    }
}
