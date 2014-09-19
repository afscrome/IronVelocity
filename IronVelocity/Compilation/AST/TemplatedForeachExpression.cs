using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class TemplatedForeachExpression : VelocityExpression
    {
        private static readonly Expression _constantOne = Expression.Constant(1);
        private static readonly Expression _constantTwo = Expression.Constant(2);

        private readonly LabelTarget _break;

        private readonly ParameterExpression _internalIndex = Expression.Parameter(typeof(int), "foreachIndex");

        public Expression Enumerable { get; private set; }
        public Expression LoopVariable { get; private set; }
        public Expression LoopIndex { get; private set; }


        public Expression BeforeAll { get; private set; }
        public Expression Before { get; private set; }
        public Expression Odd { get; private set; }
        public Expression Even { get; private set; }
        public Expression Between { get; private set; }
        public Expression After { get; private set; }
        public Expression AfterAll { get; private set; }
        public Expression Each { get; private set; }
        public Expression NoData { get; private set; }


        public TemplatedForeachExpression(Expression enumerable, Expression loopVariable, Expression beforeAll, Expression before, Expression odd, Expression even, Expression between, Expression after, Expression afterAll, Expression each, Expression noData, LabelTarget breakLabel, Expression loopIndex)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (!typeof(IEnumerable).IsAssignableFrom(enumerable.Type))
                throw new ArgumentOutOfRangeException("enumerable");

            if (loopVariable == null)
                throw new ArgumentNullException("loopVariable");
            if (loopIndex == null)
                throw new ArgumentNullException("loopIndex");
            if (breakLabel == null)
                throw new ArgumentNullException("breakLabel");

            Enumerable = enumerable;
            LoopVariable = loopVariable;
            LoopIndex = loopIndex;
            BeforeAll = beforeAll;
            Before = before;
            Odd = odd;
            Even = even;
            Between = between;
            After = after;
            AfterAll = afterAll;
            Each = each;
            NoData = noData;
            _break = breakLabel;
        }


        public override Expression Reduce()
        {
            var body = new List<Expression>();

            //Initalise the index to 0
            Expression indexInitalise = Expression.Assign(_internalIndex, Constants.Zero);
            if (LoopIndex != null)
                indexInitalise = Expression.Assign(LoopIndex, VelocityExpressions.ConvertIfNeeded(indexInitalise, LoopIndex.Type));
            body.Add(indexInitalise);

            // Do the main loop - this handles the #BeforeAll, #Before,#Odd,#Even,#After & #Each templates
            body.Add(new ForeachExpression(Enumerable, BuildItemBody(), LoopVariable, _break));


            //If we have a NoData or AfterAll template, execute that after we've finished the loop.
            if (NoData != null || AfterAll != null)
            {
                body.Add(Expression.IfThenElse(
                    Expression.Equal(_internalIndex, Constants.Zero),
                    NoData ?? Constants.EmptyExpression,
                    AfterAll ?? Constants.EmptyExpression
                ));
            }

            return Expression.Block(
                    typeof(void),
                    new[] { _internalIndex, },
                    body
                );
        }


        private Expression BuildItemBody()
        {
            var bodyExpressions = new List<Expression>();

            //First, increment the item index
            //TODO: For NVelocity compatibility, check if this is the right place to increment the index - should it instead be after the BeforeAll/Between templates>
            Expression indexIncrement = Expression.PreIncrementAssign(_internalIndex);
            if (LoopIndex != null)
                indexIncrement = Expression.Assign(LoopIndex, VelocityExpressions.ConvertIfNeeded(indexIncrement, LoopIndex.Type));
            bodyExpressions.Add(indexIncrement);

            //For the first item, output the #BeforeAll template, for all others #Between
            if (BeforeAll != null || Between != null)
            {
                bodyExpressions.Add(
                    Expression.IfThenElse(
                        Expression.Equal(_internalIndex, _constantOne),
                        BeforeAll ?? Constants.EmptyExpression,
                        Between ?? Constants.EmptyExpression
                    )
                );
            }

            //Next comes the #Before template
            if (Before != null)
                bodyExpressions.Add(Before);


            //Next come the #Odd / #Even 
            if (Even != null || Odd != null)
            {
                bodyExpressions.Add(
                    Expression.IfThenElse(
                        Expression.Equal(Constants.Zero, Expression.Modulo(_internalIndex, _constantTwo)),
                        Even ?? Constants.EmptyExpression,
                        Odd ?? Constants.EmptyExpression
                     )
                );
            }

            // Next up #Each
            if (Each != null)
                bodyExpressions.Add(Each);

            // Finally comes #After
            if (After != null)
                bodyExpressions.Add(After);

            return Expression.Block(bodyExpressions);
        }


        public override Type Type { get { return typeof(void); } }

    }

}
