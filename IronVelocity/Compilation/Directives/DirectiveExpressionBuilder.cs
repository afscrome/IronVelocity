using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public abstract class DirectiveExpressionBuilder
    {
        public abstract Expression Build(ASTDirective node, VelocityExpressionBuilder builder);
    }

}
