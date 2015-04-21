using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public class ForeachDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        public override string Name { get { return "foreach"; } }
        public override Type NVelocityDirectiveType { get { return typeof(Foreach); } }

        public override Expression Build(ASTDirective node, NVelocityNodeToExpressionConverter converter)
        {
            return new ForeachDirective(node, converter);
        }
    }


    public enum ForeachSection : int
    {
        Each = 0,
        BeforeAll = 1,
        Before = 2,
        Odd = 3,
        Even = 4,
        Between = 5,
        After = 6,
        AfterAll = 7,
        NoData = 8
    }
}
