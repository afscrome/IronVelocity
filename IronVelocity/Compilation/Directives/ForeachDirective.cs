using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public class ForeachDirective : CustomDirectiveExpression
    {
        private readonly LabelTarget _break = Expression.Label("break");
        private readonly VelocityExpressionBuilder _builder;


        public Expression Enumerable { get; private set; }
        public Expression CurrentIndex { get; private set; }
        public Expression CurrentItem { get; private set; }

        private INode _body;

        public ForeachDirective(ASTDirective node, VelocityExpressionBuilder builder)
            : base(builder)
        {
            if (node == null)
                throw new ArgumentOutOfRangeException("node");

            if (builder == null)
                throw new ArgumentOutOfRangeException("builder");

            _builder = builder;

            Enumerable = VelocityExpression.Operand(node.GetChild(2));
            _body = node.GetChild(3);

            CurrentIndex = new VariableExpression("velocityCount").ReduceExtensions();
            CurrentItem = VelocityExpression.Reference(node.GetChild(0)).ReduceExtensions();
        }


        public override Expression ProcessChildDirective(string name, INode node)
        {
            if (name == "break")
                return Expression.Break(_break);
            else
                return null;
        }

        private static Expression GetExpressionBlock(ICollection<Expression>[] parts, ForeachSection section, VelocityExpressionBuilder builder)
        {
            var expressions = parts[(int)section];
            if (expressions == null)
                return null;
            else
                return new RenderedBlock(expressions, builder);
        }

        private static ICollection<Expression>[] GetParts(IReadOnlyCollection<Expression> body)
        {
            var parts = new List<Expression>[9];
            var currentSection = ForeachSection.Each;

            foreach (var expression in body)
            {
                var seperator = expression as Directive;
                if (seperator != null)
                {
                    ForeachSection section;
                    if (Enum.TryParse(seperator.Name, true, out section))
                    {
                        currentSection = section;
                        continue;
                    }
                }
                if (parts[(int)currentSection] == null)
                    parts[(int)currentSection] = new List<Expression>();

                parts[(int)currentSection].Add(expression);

            }

            return parts;

        }

        protected override Expression ReduceInternal()
        {
            var body = _builder.GetBlockExpressions(_body);
            var parts = GetParts(body);

            var forEach = new AdvancedForeachExpression(
                enumerable: Enumerable,
                loopVariable: CurrentItem,
                loopIndex: CurrentIndex,
                breakLabel: _break,
                beforeAll: GetExpressionBlock(parts, ForeachSection.BeforeAll, _builder),
                before: GetExpressionBlock(parts, ForeachSection.Before, _builder),
                odd: GetExpressionBlock(parts, ForeachSection.Odd, _builder),
                even: GetExpressionBlock(parts, ForeachSection.Even, _builder),
                between: GetExpressionBlock(parts, ForeachSection.Between, _builder),
                after: GetExpressionBlock(parts, ForeachSection.After, _builder),
                afterAll: GetExpressionBlock(parts, ForeachSection.AfterAll, _builder),
                each: GetExpressionBlock(parts, ForeachSection.Each, _builder),
                noData: GetExpressionBlock(parts, ForeachSection.NoData, _builder)
                );


            var originalItemValue = Expression.Parameter(CurrentItem.Type, "foreachOriginalItem");
            var originalIndex = Expression.Parameter(CurrentItem.Type, "foreachOriginalVelocityIndex");

            return Expression.Block(
                typeof(void),
                new[] { originalIndex, originalItemValue },
                Expression.Assign(originalItemValue, CurrentItem),
                Expression.Assign(originalIndex, CurrentIndex),
                forEach,
                Expression.Assign(CurrentIndex, originalIndex),
                Expression.Assign(CurrentItem, originalItemValue)

                );
        }
    }

}
