using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public class BreakDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        public override string Name { get { return "break"; } }

        public override Expression Build(ASTDirective node, VelocityExpressionBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
