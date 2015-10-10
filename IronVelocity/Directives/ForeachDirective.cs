using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Directives
{
    public class ForeachDirective : CustomDirectiveExpression
    {
        private readonly LabelTarget _break = Expression.Label("break");
        //private readonly VelocityExpressionBuilder _builder;

        private static readonly Expression _nullEnumerable = Expression.Constant(null, typeof(IEnumerable));

        public Expression Enumerable { get; }
        public Expression CurrentIndex { get; }
        public Expression CurrentItem { get; }
        public Expression Body { get; }

        public ForeachDirective(Expression item, Expression enumerable, Expression body)
            : base()
        {
            if (item is ReferenceExpression)
                item = item.Reduce();

            if (item is GlobalVariableExpression)
                throw new NotSupportedException("Cannot use global variable as Foreach current item.");

            CurrentItem = item.ReduceExtensions();
            Enumerable = enumerable;
            Body = body;

            CurrentIndex = new VariableExpression("velocityCount").ReduceExtensions();
        }


        public override Expression ProcessChildDirective(string name, INode node) => null;

        private Expression GetExpressionBlock(ICollection<Expression>[] parts, ForeachSection section)
        {
            var expressions = parts[(int)section];
            if (expressions == null)
                return null;
            else
                return new RenderedBlock(expressions);
        }

        private static ICollection<Expression>[] GetParts(IReadOnlyCollection<Expression> body)
        {
            var parts = new List<Expression>[9];
            var currentSection = ForeachSection.Each;

            foreach (var expression in body)
            {
                var seperator = expression as UnrecognisedDirective;
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
            var originalItemValue = Expression.Parameter(CurrentItem.Type, "foreachOriginalItem");
            var originalIndex = Expression.Parameter(typeof(object), "foreachOriginalVelocityIndex");

            //Need to store the enumerable in a local variable so we don't end up computing it twice (once with the TypeAs check, and again with the execution)
            var localEnumerable = Expression.Parameter(typeof(IEnumerable), "foreachEnumerable");

            var body = ((RenderedBlock)Body).Children;
            var parts = GetParts(body);

            var forEach = new TemplatedForeachExpression(
                enumerable: localEnumerable,
                loopVariable: CurrentItem,
                loopIndex: CurrentIndex,
                breakLabel: _break,
                beforeAll: GetExpressionBlock(parts, ForeachSection.BeforeAll),
                before: GetExpressionBlock(parts, ForeachSection.Before),
                odd: GetExpressionBlock(parts, ForeachSection.Odd),
                even: GetExpressionBlock(parts, ForeachSection.Even),
                between: GetExpressionBlock(parts, ForeachSection.Between),
                after: GetExpressionBlock(parts, ForeachSection.After),
                afterAll: GetExpressionBlock(parts, ForeachSection.AfterAll),
                each: GetExpressionBlock(parts, ForeachSection.Each),
                noData: GetExpressionBlock(parts, ForeachSection.NoData)
            );




            var loopExecution = Expression.Block(
                typeof(void),
                new[] { originalIndex, originalItemValue },
                Expression.Assign(originalItemValue, CurrentItem),
                Expression.Assign(originalIndex, CurrentIndex),
                forEach,
                Expression.Assign(CurrentIndex, originalIndex),
                Expression.Assign(CurrentItem, originalItemValue)
            );


            return new TemporaryVariableScopeExpression(
                localEnumerable,
                Expression.Block(
                    Expression.Assign(localEnumerable, Expression.TypeAs(Enumerable, typeof(IEnumerable))),
                    Expression.IfThenElse(
                        Expression.Equal(localEnumerable, _nullEnumerable),
                        forEach.NoData ?? Constants.EmptyExpression,
                        loopExecution
                    )
                )
            );

        }
    }

}
