using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.Directives
{
    public class ForEachDirectiveExpressionBuilder : DirectiveExpressionBuilder
    {
        private static readonly MethodInfo _enumeratorMethodInfo = typeof(IEnumerable).GetMethod("GetEnumerator", new Type[] { });
        private static readonly MethodInfo _moveNextMethodInfo = typeof(IEnumerator).GetMethod("MoveNext", new Type[] { });
        private static readonly PropertyInfo _currentPropertyInfo = typeof(IEnumerator).GetProperty("Current");


        public override Expression Build(ASTDirective node)
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

            var loopVariable = VelocityASTConverter.Reference(node.GetChild(0), false);

            //TODO: Hack until Foreach is moved to an Extension expression node
            var temp = loopVariable as IronVelocity.Compilation.AST.DynamicReference;
            if (temp != null)
                loopVariable = temp.Value;
            var temp2 = loopVariable as IronVelocity.Compilation.AST.VariableReference;
            if (temp2 != null)
                loopVariable = temp2.Reduce();
            var enumerable =  AST.ConversionHelpers.Operand(node.GetChild(2));
            
            var parts = new List<Expression>[9];
            var currentSection = ForEachSection.Each;
            foreach (var expression in AST.ConversionHelpers.GetBlockExpressions(node.GetChild(3)))
            {
                var seperator = expression as Directive;
                if (seperator != null)
                {
                    ForEachSection section;
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

            var index = new AST.VariableReference("velocityCount").Reduce();


            //For the first item, output the #BeforeAll template, for all others #Between
            var bodyPrefix = Expression.IfThenElse(
                    Expression.Equal(Expression.Convert(index, typeof(int)), Expression.Constant(1)),
                    GetExpressionBlock(parts[(int)ForEachSection.BeforeAll]),
                    GetExpressionBlock(parts[(int)ForEachSection.Between])
                );

            var oddEven = Expression.IfThenElse(
                Expression.Equal(Expression.Constant(0), Expression.Modulo(VelocityExpressions.ConvertIfNeeded(index, typeof(int)), Expression.Constant(2))),
                GetExpressionBlock(parts[(int)ForEachSection.Even]),
                GetExpressionBlock(parts[(int)ForEachSection.Odd])
                );

            var noData = GetExpressionBlock(parts[(int)ForEachSection.NoData]);
            var loopSuffix = Expression.IfThenElse(
                    Expression.Equal(Expression.Constant(0), VelocityExpressions.ConvertIfNeeded(index, typeof(int))),
                    noData,
                    GetExpressionBlock(parts[(int)ForEachSection.AfterAll])
                );


            var bodyExpressions = new List<Expression>();
            bodyExpressions.Add(bodyPrefix);
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForEachSection.Before]));
            bodyExpressions.Add(oddEven);
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForEachSection.Each]));
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForEachSection.After]));

            var body = Expression.Block(bodyExpressions);
            return ForeachExpression(enumerable, body, loopVariable, index, loopSuffix, noData);
        }

        private static Expression GetExpressionBlock(IEnumerable<Expression> expressions)
        {
            if (expressions == null)
                return Expression.Default(typeof(void));
            else
                return new RenderedBlock(expressions);
        }


        private static Expression ForeachExpression(Expression enumerable, Expression body, Expression currentItem, Expression currentIndex, Expression loopSuffix, Expression noData)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (body == null)
                throw new ArgumentNullException("body");
            if (currentItem == null)
                throw new ArgumentNullException("currentItem");
            if (currentIndex == null)
                throw new ArgumentNullException("currentIndex");

            if (currentItem.Type != typeof(object))
                throw new ArgumentOutOfRangeException("currentItem");
            if (currentIndex.Type != typeof(object) && currentIndex.Type != typeof(int))
                throw new ArgumentOutOfRangeException("currentIndex");

            if (enumerable.Type != typeof(IEnumerable))
                enumerable = Expression.TypeAs(enumerable, typeof(IEnumerable));

            var enumerator = Expression.Parameter(typeof(IEnumerator), "enumerator");
            var @break = Expression.Label("break");
            var @continue = Expression.Label("continue");

            var localIndex = Expression.Parameter(typeof(int), "foreachIndex");
            var originalItemValue = Expression.Parameter(currentItem.Type, "foreachOriginalItem");
            var originalIndex = Expression.Parameter(currentItem.Type, "foreachOriginalVelocityIndex");


            var loop = Expression.IfThenElse(
                Expression.TypeIs(enumerable, typeof(IEnumerable)),                
                Expression.Block(
                    typeof(void),
                    new[] { enumerator, localIndex, originalItemValue, originalIndex },
                //Store original item & index to revert to after the loop
                    Expression.Assign(originalItemValue, currentItem),
                    Expression.Assign(originalIndex, currentIndex),
                //Initalise the index
                    Expression.Assign(currentIndex, VelocityExpressions.ConvertIfNeeded(Expression.Assign(localIndex, Expression.Constant(0)), currentIndex.Type)),
                //Can only attempt foreach if the input implements IEnumerable
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
                            ),
                            loopSuffix,
  //                      )
  //                  ),
                //Revert current item & index to original values
                    Expression.Assign(currentIndex, originalIndex),
                    Expression.Assign(currentItem, originalItemValue)
                ),
                noData
                );

            return loop;
        }
    }

    public class ForEachSectionExpressionBuilder : DirectiveExpressionBuilder
    {
        private readonly ForEachSection _part;
        public ForEachSectionExpressionBuilder(ForEachSection part)
        {
            _part = part;
        }

        public override Expression Build(ASTDirective node)
        {
            return new ForEachPartSeparatorExpression(_part);
        }

    }

    public class ForEachPartSeparatorExpression : Expression
    {
        public ForEachPartSeparatorExpression(ForEachSection part)
        {
            Part = part;
        }
        public ForEachSection Part { get; private set; }

        public override Type Type { get { return typeof(void); } }
        public override ExpressionType NodeType { get { return ExpressionType.Extension; } }
        public override bool CanReduce { get { return false; } }
    }

    public enum ForEachSection : int
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
