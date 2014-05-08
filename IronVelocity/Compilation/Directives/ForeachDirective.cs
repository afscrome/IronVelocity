using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.Directives
{
    public class ForeachDirective : CustomDirective
    {
        private static readonly MethodInfo _enumeratorMethodInfo = typeof(IEnumerable).GetMethod("GetEnumerator", new Type[] { });
        private static readonly MethodInfo _moveNextMethodInfo = typeof(IEnumerator).GetMethod("MoveNext", new Type[] { });
        private static readonly PropertyInfo _currentPropertyInfo = typeof(IEnumerator).GetProperty("Current");

        private readonly VelocityExpressionBuilder _builder;
        private readonly LabelTarget _break = Expression.Label("break");

        public ForeachDirective(ASTDirective node, VelocityExpressionBuilder builder)
            : base(node, builder)
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

            _builder = builder;
        }

        protected override Expression ReduceInternal()
        {
            var loopVariable = new DynamicReference(Node.GetChild(0));

            var enumerable = VelocityExpressionBuilder.Operand(Node.GetChild(2));


            var index = new VariableReference("velocityCount");
            var parts = GetParts();

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

            var noData = GetExpressionBlock(parts[(int)ForeachSection.NoData]);
            var loopSuffix = Expression.IfThenElse(
                    Expression.Equal(Expression.Constant(0), VelocityExpressions.ConvertIfNeeded(index, typeof(int))),
                    noData,
                    GetExpressionBlock(parts[(int)ForeachSection.AfterAll])
                );


            var bodyExpressions = new List<Expression>();
            bodyExpressions.Add(bodyPrefix);
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForeachSection.Before]));
            bodyExpressions.Add(oddEven);
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForeachSection.Each]));
            bodyExpressions.Add(GetExpressionBlock(parts[(int)ForeachSection.After]));

            var body = Expression.Block(bodyExpressions);
            return ForeachExpression(enumerable, body, loopVariable, index, loopSuffix, noData);
        }

        private Expression GetExpressionBlock(IEnumerable<Expression> expressions)
        {
            if (expressions == null)
                return Expression.Default(typeof(void));
            else
                return new RenderedBlock(expressions, _builder);
        }


        private Expression ForeachExpression(Expression enumerable, Expression body, Expression currentItem, Expression currentIndex, Expression loopSuffix, Expression noData)
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

            currentIndex = currentIndex.ReduceExtensions();
            currentItem = currentItem.ReduceExtensions();

            var enumerator = Expression.Parameter(typeof(IEnumerator), "enumerator");
            var @continue = Expression.Label("continue");

            var localIndex = Expression.Parameter(typeof(int), "foreachIndex");
            var originalItemValue = Expression.Parameter(currentItem.Type, "foreachOriginalItem");
            var originalIndex = Expression.Parameter(currentItem.Type, "foreachOriginalVelocityIndex");

            var foreachLoop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.IsTrue(Expression.Call(enumerator, _moveNextMethodInfo)),
                        Expression.Block(
                            Expression.Assign(currentItem, Expression.Property(enumerator, _currentPropertyInfo)),
                            Expression.Assign(currentIndex, VelocityExpressions.ConvertIfNeeded(Expression.PreIncrementAssign(localIndex), currentIndex.Type)),
                            body
                        ),
                        Expression.Break(_break)
                    ),
                    _break,
                    @continue
                );

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
                            foreachLoop,
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


        private ICollection<Expression>[] GetParts()
        {
            var parts = new List<Expression>[9];
            var currentSection = ForeachSection.Each;
            
            foreach (var expression in _builder.GetBlockExpressions(Node.GetChild(3)))
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


        public override Expression ProcessChildDirective(string name, INode node)
        {
            if (name == "break")
                return Expression.Break(_break);
            else
                return null;
        }
    }
}
