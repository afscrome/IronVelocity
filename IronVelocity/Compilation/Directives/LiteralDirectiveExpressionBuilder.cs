using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.Directives
{
    public class LiteralDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        public override Expression Build(ASTDirective node, VelocityExpressionBuilder builder)
        {
            return new LiteralDirective(node, builder);
        }
    }
}
