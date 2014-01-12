using NVelocity.Runtime.Parser.Node;
using System.Linq.Expressions;

namespace IronVelocity.Directives
{
    public abstract class DirectiveExpressionBuilder
    {
        public abstract Expression Build(ASTDirective node, VelocityASTConverter converter);
    }





}
