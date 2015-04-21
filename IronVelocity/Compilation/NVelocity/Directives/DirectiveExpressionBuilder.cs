using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public abstract class DirectiveExpressionBuilder
    {
        public abstract string Name { get; }
        public abstract Type NVelocityDirectiveType { get; }
        public abstract Expression Build(ASTDirective node, NVelocityNodeToExpressionConverter converter);
    }

}
