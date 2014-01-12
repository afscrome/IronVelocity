using System;
using System.Linq.Expressions;

namespace IronVelocity.Directives
{
    public class BreakDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        public override Expression Build(NVelocity.Runtime.Parser.Node.ASTDirective node, VelocityASTConverter converter)
        {
            throw new NotImplementedException();
        }
    }
}
