using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Directives
{
    public class ForeachDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        private static readonly MethodInfo _enumeratorMethodInfo = typeof(IEnumerable).GetMethod("GetEnumerator", new Type[] { });
        private static readonly MethodInfo _moveNextMethodInfo = typeof(IEnumerator).GetMethod("MoveNext", new Type[] { });
        private static readonly PropertyInfo _currentPropertyInfo = typeof(IEnumerator).GetProperty("Current");


        public override Expression Build(ASTDirective node, VelocityASTConverter converter)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!node.DirectiveName.Equals("foreach", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount < 4)
                throw new ArgumentOutOfRangeException("node");

            var inNode = node.GetChild(1) as ASTWord;
            if (inNode == null || !inNode.Literal.Equals("in"))
                throw new ArgumentOutOfRangeException("node");

            var loopVariable = converter.Reference(node.GetChild(0), false);
            var enumerable = converter.Operand(node.GetChild(2));

            var parts = new List<Expression>[9];
            var currentSection = ForeachSection.Each;
            foreach (var expression in converter.GetBlockExpressions(node.GetChild(3), true))
            {
                var seperator = expression as ForeachPartSeperatorExpression;
                if (seperator != null)
                {
                    currentSection = seperator.Part;
                }
                else
                {
                    if (parts[(int)currentSection] == null)
                        parts[(int)currentSection] = new List<Expression>();

                    parts[(int)currentSection].Add(expression);
                }

            }

            var index = converter.GetVariable("velocityCount");


            //For the first item, output the #BeforeAll template, for all others #Between
            var bodyPrefix = Expression.IfThenElse(
                    Expression.Equal(Expression.Convert(index, typeof(int)), Expression.Constant(1)),
                    GetExpressionBlock(parts[(int)ForeachSection.BeforeAll]),
                    GetExpressionBlock(parts[(int)ForeachSection.Between])
                );

            var oddEven = Expression.IfThenElse(
                Expression.Equal(Expression.Constant(0), Expression.Modulo(VelocityExpressions.ConvertIfNeeded(index, typeof(int)), Expression.Constant(2))),
                GetExpressionBlock(parts[(int)ForeachSection.Even]),
                GetExpressionBlock(parts[(int)ForeachSection.Odd])
                );

            var loopSuffix = Expression.IfThenElse(
                    Expression.Equal(Expression.Constant(0), VelocityExpressions.ConvertIfNeeded(index, typeof(int))),
                    GetExpressionBlock(parts[(int)ForeachSection.NoData]),
                    GetExpressionBlock(parts[(int)ForeachSection.AfterAll])
                );


            var bodyExpressions = new List<Expression>();
            bodyExpressions.Add(bodyPrefix);
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForeachSection.Before]));
            bodyExpressions.Add(oddEven);
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForeachSection.Each]));
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForeachSection.After]));

            var body = Expression.Block(bodyExpressions);
            return ForeachExpression(enumerable, body, loopVariable, index, loopSuffix);
        }

        private Expression GetExpressionBlock(IEnumerable<Expression> expressions)
        {
            if (expressions == null)
                return Expression.Default(typeof(void));
            else
                return Expression.Block(typeof(void), expressions);
        }


        private static Expression ForeachExpression(Expression enumerable, Expression body, Expression currentItem, Expression currentIndex, Expression loopSuffix)
        {
            if (enumerable == null)
                throw new ArgumentNullException("collection");
            if (body == null)
                throw new ArgumentNullException("body");
            if (currentItem == null)
                throw new ArgumentNullException("currentItem");
            if (currentIndex == null)
                throw new ArgumentNullException();

            if (currentItem.Type != typeof(object))
                throw new ArgumentOutOfRangeException("currentItem");
            if (currentIndex.Type != typeof(object) && currentIndex.Type != typeof(int))
                throw new ArgumentOutOfRangeException("currentIndex");

            if (enumerable.Type != typeof(IEnumerable))
            {
                //if (typeof(IEnumerable).IsAssignableFrom(enumerable.Type))
                enumerable = Expression.Convert(enumerable, typeof(IEnumerable));
                //else
                //    throw new ArgumentOutOfRangeException("collection", "Collection expression must be assignable to IEnumerable");
            }

            var enumerator = Expression.Parameter(typeof(IEnumerator), "enumerator");
            var @break = Expression.Label("break");
            var @continue = Expression.Label("continue");

            var localIndex = Expression.Parameter(typeof(int), "index");
            var originalItemValue = Expression.Parameter(currentItem.Type, "originalItem");
            var originalIndex = Expression.Parameter(currentItem.Type, "originalVelocityIndex");


            var loop = Expression.Block(
                    typeof(void),
                    new[] { enumerator, localIndex, originalItemValue, originalIndex },
                //Store original item & index to revert to after the loop
                    Expression.Assign(originalItemValue, currentItem),
                    Expression.Assign(originalIndex, currentIndex),
                //Initalise the index
                    Expression.Assign(currentIndex, VelocityExpressions.ConvertIfNeeded(Expression.Assign(localIndex, Expression.Constant(0)), currentIndex.Type)),
                //Can only attempt foreach if the input implements IEnumerable
                    Expression.IfThen(
                        Expression.TypeIs(enumerable, typeof(IEnumerable)),
                        Expression.Block(
                //Get the enumerator
                            Expression.Assign(enumerator, Expression.Call(enumerable, _enumeratorMethodInfo)),
                // Loop through the enumerator
                            Expression.Loop(
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(enumerator, _moveNextMethodInfo)),
                                    Expression.Block(
                                        Expression.Assign(currentItem, Expression.Property(enumerator, _currentPropertyInfo)),
                                        Expression.Assign(currentIndex, VelocityExpressions.ConvertIfNeeded(Expression.PreIncrementAssign(localIndex), currentIndex.Type)),
                                        body
                                    ),
                                    Expression.Break(@break)
                                ),
                                @break,
                                @continue
                            )
                        )
                    ),
                    loopSuffix,
                //Revert current item & index to original values
                    Expression.Assign(currentIndex, originalIndex),
                    Expression.Assign(currentItem, originalItemValue)
                );

            return loop;
        }
    }

    public class ForeachSectionExpressionBuilder : DirectiveExpressionBuilder
    {
        private readonly ForeachSection _part;
        public ForeachSectionExpressionBuilder(ForeachSection part)
        {
            _part = part;
        }

        public override Expression Build(ASTDirective node, VelocityASTConverter converter)
        {
            return new ForeachPartSeperatorExpression(_part);
        }

    }

    public class ForeachPartSeperatorExpression : Expression
    {
        public ForeachPartSeperatorExpression(ForeachSection part)
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
