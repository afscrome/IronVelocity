using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.Directives
{
    public class ForeachDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        public override Expression Build(ASTDirective node, VelocityExpressionBuilder builder)
        {
            return new ForeachDirective(node, builder);
        }

    }

    public class ForeachSectionExpressionBuilder : DirectiveExpressionBuilder
    {
        private readonly ForeachSection _part;
        public ForeachSectionExpressionBuilder(ForeachSection part)
        {
            _part = part;
        }

        public override Expression Build(ASTDirective node, VelocityExpressionBuilder builder)
        {
            return new ForeachPartSeparatorExpression(_part);
        }

    }

    public class ForeachPartSeparatorExpression : Expression
    {
        public ForeachPartSeparatorExpression(ForeachSection part)
        {
            Part = part;
        }
        public ForeachSection Part { get; private set; }

        public override Type Type { get { return typeof(void); } }
        public override ExpressionType NodeType { get { return ExpressionType.Extension; } }
        public override bool CanReduce { get { return false; } }
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
