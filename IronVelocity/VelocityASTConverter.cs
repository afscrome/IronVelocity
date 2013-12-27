using IronVelocity.Binders;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using VelocityExpressionTree.Binders;

namespace IronVelocity
{
    public class VelocityASTConverter
    {
        private static readonly MethodInfo _appendMethodInfo = typeof(StringBuilder).GetMethod("Append", new[] { typeof(object) });
        private static readonly MethodInfo _additionMethodInfo = typeof(VelocityOperators).GetMethod("Addition", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _subtractionMethodInfo = typeof(VelocityOperators).GetMethod("Subtraction", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _multiplicationMethodInfo = typeof(VelocityOperators).GetMethod("Multiplication", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _divisionMethodInfo = typeof(VelocityOperators).GetMethod("Division", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _moduloMethodInfo = typeof(VelocityOperators).GetMethod("Modulo", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _coerceObjectToBooleanMethodInfo = typeof(VelocityCoercion).GetMethod("CoerceObjectToBoolean", new[] { typeof(object) });
         

        private static readonly Expression TrueExpression = Expression.Constant(true);
        private static readonly Expression FalseExpression = Expression.Constant(false);


        private IDictionary<string, VelocityGetMemberBinder> _binders = new Dictionary<string, VelocityGetMemberBinder>(StringComparer.OrdinalIgnoreCase);
        private VelocityGetMemberBinder GetGetMemberBinder(string propertyName)
        {
            VelocityGetMemberBinder binder;
            if (!_binders.TryGetValue(propertyName, out binder))
            {
                binder = new VelocityGetMemberBinder(propertyName);
                _binders[propertyName] = binder;
            }
            return binder;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [CLSCompliant(false)]
        public Expression BuildExpressionTree(ASTprocess ast)
        {
            if (ast == null)
                throw new ArgumentNullException("ast");

            if (!(ast is ASTprocess))
                throw new ArgumentOutOfRangeException("ast");

            var expressions = GetBlockExpressions(ast);

            if (!expressions.Any())
                expressions = new[] { Expression.Empty() };

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

            //ASTprocess is a special case for the root, otherwise it behaves exactly like ASTBlock
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
            var dotIndex = name.IndexOf('.');
            if (dotIndex > 0)
                name = name.Substring(0, dotIndex);

            Expression expr = GetLocal(name);

            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
                if (child is ASTIdentifier)
                    expr = Identifier(child, expr);
                else if (child is ASTMethod)
                    expr = Method(child, expr);
                else
                    throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);
            }

            return expr;
        }


        private static Expression Method(INode node, Expression parent)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTMethod))
                throw new ArgumentOutOfRangeException("node");

            var paramBoundary = node.Literal.IndexOf('(');
            var methodName = node.Literal.Substring(0, paramBoundary);

            return Expression.Dynamic(
                new VelocityInvokeMemberBinder(methodName, new CallInfo(0)),
                typeof(object),
                parent
            );
        }

        private Expression Identifier(INode node, Expression parent)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTIdentifier))
                throw new ArgumentOutOfRangeException("node");

            var propertyName = node.Literal;

            return Expression.Dynamic(
                GetGetMemberBinder(propertyName),
                typeof(object),
                parent
            );
        }


        private static Expression NumberLiteral(INode node)
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
        /// Helper to build an If or an IfElse statement based on whether we have the falseContent block is not null
        /// </summary>
        /// <returns></returns>
        private static Expression If(Expression condition, Expression trueContent, Expression falseContent)
        {
            if (condition.Type != typeof(bool))
                condition = Expression.Call(_coerceObjectToBooleanMethodInfo, condition);

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
            //Literals
            if (child is ASTTrue)
                return TrueExpression;
            else if (child is ASTFalse)
                return FalseExpression;
            else if (child is ASTNumberLiteral)
                return NumberLiteral(child);
            else if (child is ASTStringLiteral)
                return String(child);
            //Code
            else if (child is ASTAssignment)
                return Assign(child);
            else if (child is ASTReference)
                return Reference(child);
            //Mathematical Operations
            else if (child is ASTAddNode)
                return Addition(child);
            else if (child is ASTSubtractNode)
                return Subtraction(child);
            else if (child is ASTMulNode)
                return Multiplication(child);
            else if (child is ASTDivNode)
                return Division(child);
            else if (child is ASTModNode)
                return Modulo(child);
            else
                throw new NotSupportedException("Node type not supported in an expression: " + child.GetType().Name);
        }

        private Expression String(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTStringLiteral))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount > 0)
                throw new NotImplementedException("Not supporting interpolated strings yet");

            return Expression.Constant(node.Literal.Substring(1, node.Literal.Length - 2));
        }

        private Expression Markup(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            return Output(Expression.Constant(node.Literal));
        }

        private static Expression Output(Expression expression)
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

           
            if (left.Type != right.Type)
            {
                //This shouldn't really be happening as we should only be assigning to objects, but just in case...
                if (!left.Type.IsAssignableFrom(right.Type))
                    throw new InvalidOperationException("Cannot assign from type '{0}' to '{1}'");

                right = Expression.Convert(right, left.Type);
            }

            var tempResult = Expression.Parameter(typeof(object));

            /* One of the nucanses of velocity is that if the right evaluates to null,
             * Thus we can't simply return an assignment expression.
             * The resulting expression looks as follows:P
             *     .set $tempResult = right;
             *     .if ($tempResult != null){
             *         Assign(left, right)
             *     }
             */
            return Expression.Block(new[] { tempResult },
                //Store the result of the right hand side in to a temporary variable
                Expression.Assign(tempResult, right),
                Expression.IfThen(
                //If the temporary variable is not equal to null
                    Expression.NotEqual(tempResult, Constants.NullExpression),
                //Make the assignment
                    Expression.Assign(left, right)
                )
            );
            //return Expression.Assign(left, right);
        }
        #endregion


        #region Numeric Operators

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
            return BinaryMathematicalExpression(Expression.Multiply, left, right, _multiplicationMethodInfo);
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
            return BinaryMathematicalExpression(Expression.Divide, left, right, _divisionMethodInfo);
        }

        /// <summary>
        /// Builds a modulo expression from an ASTModNode
        /// </summary>
        /// <param name="node">The ASTModNode to build the modulo Expression from</param>
        /// <returns>An expression representing the modulo operation</returns>
        private Expression Modulo(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTModNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryMathematicalExpression(Expression.Divide, left, right, _moduloMethodInfo);
        }


        private static Expression BinaryMathematicalExpression(Func<Expression, Expression, MethodInfo, Expression> generator, Expression left, Expression right, MethodInfo implementation)
        {
            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            if (left.Type != typeof(object))
                left = Expression.Convert(left, typeof(object));

            if (right.Type != typeof(object))
                right = Expression.Convert(right, typeof(object));

            return generator(left, right, implementation);
        }

        #endregion

    }
}
