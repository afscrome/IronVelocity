using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IronVelocity
{
    public class VelocityASTConverter
    {
        private static readonly MethodInfo _appendMethodInfo = typeof(StringBuilder).GetMethod("Append", new[] { typeof(object) });
        private static readonly MethodInfo _additionMethodInfo = typeof(VelocityOperators).GetMethod("Addition", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _subtractionMethodInfo = typeof(VelocityOperators).GetMethod("Subtraction", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _multiplicationMethodInfo = typeof(VelocityOperators).GetMethod("Multiplication", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _divisionMethodInfo = typeof(VelocityOperators).GetMethod("Division", new[] { typeof(object), typeof(object) });


        private static readonly Expression TrueExpression = Expression.Constant(true);
        private static readonly Expression FalseExpression = Expression.Constant(false);
        private static readonly Expression NullValueExpression = Expression.Constant(null);

        public Expression BuildExpressionTree(ASTprocess ast)
        {
            if (ast == null)
                throw new ArgumentNullException("node");

            if (!(ast is ASTprocess))
                throw new ArgumentOutOfRangeException("node");

            var expressions = GetBlockExpressions(ast);

            if (!expressions.Any())
                expressions = new[] { Expression.Empty()};

            return Expression.Block(_locals.Values, expressions);
        }


        private IDictionary<string, ParameterExpression> _locals = new Dictionary<string, ParameterExpression>(StringComparer.OrdinalIgnoreCase);

        private ParameterExpression GetLocal(string name)
        {
            ParameterExpression local;
            if (!_locals.TryGetValue(name, out local))
            {
                local = Expression.Parameter(typeof(object), name);
                _locals[name] = local;
            }

            return local;
        }



        private IEnumerable<Expression> GetBlockExpressions(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            //ASTprocess is a special case for the root, otherwise it behaves exactlyu like ASTBlock
            if (!(node is ASTBlock || node is ASTprocess))
                throw new ArgumentOutOfRangeException("node");

            var expressions = new List<Expression>();

            foreach (var child in node.GetChildren())
            {
                if (child is ASTText)
                    expressions.Add(Markup(child));
                else if (child is ASTReference)
                    expressions.Add(Output(Reference(child)));
                else if (child is ASTIfStatement)
                    expressions.Add(IfStatement(child));
                else if (child is ASTSetDirective)
                    expressions.Add(Set(child));
                else if (child is ASTComment)
                    continue;
                else
                    throw new NotSupportedException("Node type not supported in a block: " + child.GetType().Name);

            }

            return expressions;
        }

        private Expression Block(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTBlock))
                throw new ArgumentOutOfRangeException("node");

            var children = GetBlockExpressions(node);

            if (children.Any())
                return Expression.Block(children);
            else
                return Expression.Empty();
        }

        private Expression Set(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTSetDirective))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException("node", "Expected only one child");

            return Expr(node.GetChild(0));
        }





        private Expression Reference(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTReference))
                throw new ArgumentOutOfRangeException("node");

            //Remove $ or $! prefix to get just the variable name
            var name = node.Literal.TrimStart('$', '!');
            var expr = GetLocal(name);

            if (node.ChildrenCount > 0)
                throw new NotImplementedException();

            return expr;
        }

        private Expression NumberLiteral(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTNumberLiteral))
                throw new ArgumentOutOfRangeException("node");

            return Expression.Constant(int.Parse(node.Literal));
        }

        private Expression IfStatement(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTIfStatement))
                throw new ArgumentOutOfRangeException("node");


            var condition = Expr(node.GetChild(0));
            var trueContent = Block(node.GetChild(1));
            Expression falseContent = null;

            //Build the false expression recursively from the bottom up
            for (int i = node.ChildrenCount - 1; i > 1; i--)
            {
                var child = node.GetChild(i);
                if (child is ASTElseStatement)
                {
                    if (falseContent != null)
                        throw new InvalidOperationException("Cannot have two 'else' statements");

                    if (child.ChildrenCount != 1)
                        throw new InvalidOperationException("Expected ASTElseStatement to only have 1 child");

                    falseContent = Block(child.GetChild(0));
                }
                else if (child is ASTElseIfStatement)
                {
                    var innerCondition = Expr(child.GetChild(0));
                    var innerContent = Block(child.GetChild(1));

                    falseContent = If(innerCondition, innerContent, falseContent);
                }
                else

                    throw new InvalidOperationException("Expected: ASTElseStatement, Actual: " + child.GetType().Name);
            }
            return If(condition, trueContent, falseContent);
        }

        /// <summary>
        /// Helper to build an If or an IfElse statement based on whether we hvae the falseContent block is not null
        /// </summary>
        /// <returns></returns>
        private Expression If(Expression condition, Expression trueContent, Expression falseContent)
        {
            return falseContent != null
                ? Expression.IfThenElse(condition, trueContent, falseContent)
                : Expression.IfThen(condition, trueContent);

        }

        private Expression Expr(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTExpression))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException("node", "Only expected one child");

            var child = node.GetChild(0);
            if (child is ASTTrue)
                return TrueExpression;
            else if (child is ASTFalse)
                return FalseExpression;
            else if (child is ASTAssignment)
                return Assign(child);
            else if (child is ASTNumberLiteral)
                return NumberLiteral(child);
            else if (child is ASTStringLiteral)
                return String(child);
            else if (child is ASTReference)
                return Reference(child);
            else if (child is ASTAddNode)
                return Addition(child);
            else if (child is ASTSubtractNode)
                return Subtraction(child);
            else
                throw new NotSupportedException("Node type not supported in an expression: " + child.GetType().Name);
        }



        public Expression String(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTStringLiteral))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount > 0)
                throw new NotImplementedException("Not supporting interpolated strings yet");

            return Expression.Constant(node.Literal.Substring(1, node.Literal.Length -2));
        }

        public Expression Markup(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            return Output(Expression.Constant(node.Literal));
        }

        [Obsolete("Not entirely keen on this", false)]
        public Expression Output(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return Expression.Call(Constants.OutputParameter, _appendMethodInfo, expression);

        }




        #region Binary Operations


        private void GetBinaryExpressionOperands(INode node, out Expression left, out Expression right)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.ChildrenCount != 2)
                throw new NotImplementedException("Expected exactly two children for a binary expression");

            left = Operand(node.GetChild(0));
            right = Operand(node.GetChild(1));
        }



        private Expression Operand(INode node)
        {
            if (node is ASTNumberLiteral)
                return NumberLiteral(node);
            else if (node is ASTStringLiteral)
                return String(node);
            else if (node is ASTReference)
                return Reference(node);
            else if (node is ASTExpression)
                return Expr(node);
            else
                throw new NotSupportedException("Node type not supported in an expression: " + node.GetType().Name);
        }


        /// <summary>
        /// Builds an addition expression from an ASTAddNode
        /// </summary>
        /// <param name="node">The ASTAddNode to build the addition Expression from</param>
        /// <returns>An expression representing the addition operation</returns>
        private Expression Addition(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTAddNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryMathematicalExpression(Expression.Add, left, right, _additionMethodInfo);
        }

        /// <summary>
        /// Builds a subtraction expression from an ASTSubtractNode
        /// </summary>
        /// <param name="node">The ASTSubtractNode to build the subtraction Expression from</param>
        /// <returns>An expression representing the subtraction operation</returns>
        private Expression Subtraction(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTSubtractNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryMathematicalExpression(Expression.Subtract, left, right, _subtractionMethodInfo);
        }

        /// <summary>
        /// Builds a multiplication expression from an ASTMulNode
        /// </summary>
        /// <param name="node">The ASTMulNode to build the multiplication Expression from</param>
        /// <returns>An expression representing the multiplication operation</returns>
        private Expression Multiplication(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTMulNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryMathematicalExpression(Expression.Multiply, left, right, _additionMethodInfo);
        }

        /// <summary>
        /// Builds a division expression from an ASTDivNode
        /// </summary>
        /// <param name="node">The AstDivNode to build the division Expression from</param>
        /// <returns>An expression representing the division operation</returns>
        private Expression Division(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTDivNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryMathematicalExpression(Expression.Divide, left, right, _additionMethodInfo);
        }


        private Expression BinaryMathematicalExpression(Func<Expression, Expression, MethodInfo, Expression> generator, Expression left, Expression right, MethodInfo implementation)
        {
            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everyting is converted to object
            if (left.Type != typeof(object))
                left = Expression.Convert(left, typeof(object));

            if (right.Type != typeof(object))
                right = Expression.Convert(right, typeof(object));

            return generator(left, right, implementation);
        }

        /// <summary>
        /// Builds the expression tree from an ASTAssignment
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private Expression Assign(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTAssignment))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);


            //This shouldn't really be happening as we should only be assigning to objects, but just in case...
            if (!left.Type.IsAssignableFrom(right.Type))
                throw new InvalidOperationException("Cannot assign from type '{0}' to '{1}'");

            if (left.Type != right.Type && right.Type.IsPrimitive)
                right = Expression.Convert(right, typeof(object));

            var tempResult = Expression.Parameter(typeof(object));

            /* One of the nucanses of velocity is that if the right evaluates to null,
             * Thus we can't simply return an assignment expression.
             * The resulting expression looks as follows:P
             *     .set $tempResult = right;
             *     .if ($tempResult != null){
             *         Assign(left, right)
             *     }
             */
            return Expression.Block(new[] { tempResult},
                //Store the result of the right hand side in to a temporary variable
                Expression.Assign(tempResult, right),
                Expression.IfThen(
                    //If the temporary variable is not equal to null
                    Expression.NotEqual(tempResult, NullValueExpression),
                    //Make the assignment
                    Expression.Assign(left, right)
                )
            );
            //return Expression.Assign(left, right);
        }
        #endregion
    }
}
