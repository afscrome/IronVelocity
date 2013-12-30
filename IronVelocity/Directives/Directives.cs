using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Directivces
{
    public abstract class DirectiveExpressionBuilder
    {
        //public abstract string Name { get; }
        //public abstract DirectiveType Type { get; }

        public virtual bool AcceptParams { get { return true; } }

        public virtual bool SupportsNestedDirective(string name)
        {
            return false;
        }

        public virtual DirectiveExpressionBuilder CreateNestedDirective(string name)
        {
            return null;
        }

        public abstract Expression Build(ASTDirective node, VelocityASTConverter converter);

    }

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
            var enumerable = converter.Statement(node.GetChild(2));
            var body = converter.Block(node.GetChild(3));
            var index = converter.GetLocal("velocityCount");
            return ForeachExpression(enumerable, body, loopVariable, index);
        }

        private static Expression ForeachExpression(Expression enumerable, Expression body, Expression currentItem, Expression currentIndex)
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
                    new[] { enumerator, localIndex, originalItemValue, originalIndex },
                    //Store original item & index to revert to after the loop
                    Expression.Assign(originalItemValue, currentItem),
                    Expression.Assign(originalIndex, currentIndex),
                    //Get the enumerator
                    Expression.Assign(enumerator, Expression.Call(enumerable, _enumeratorMethodInfo)),
                    Expression.Assign(localIndex, Expression.Constant(0, typeof(int))),
                    //Enumerate through
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.IsTrue(Expression.Call(enumerator, _moveNextMethodInfo)),
                            Expression.Block(
                                Expression.Assign(currentItem, Expression.Property(enumerator, _currentPropertyInfo)),
                                Expression.Assign(currentIndex, Expression.Convert(Expression.PreIncrementAssign(localIndex), typeof(object))),
                                body
                            ),
                            Expression.Break(@break)
                        ),
                        @break,
                        @continue
                    ),
                    //Revert current item & index to original values
                    Expression.Assign(currentItem, originalItemValue),
                    Expression.Assign(currentIndex, originalIndex)
                );

            return Expression.IfThen(
                Expression.TypeIs(enumerable, typeof(IEnumerable)),
                loop
            );

        }




    }
}
