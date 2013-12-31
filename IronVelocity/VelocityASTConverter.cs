using IronVelocity.Binders;
using IronVelocity.Directivces;
using IronVelocity.RuntimeHelpers;
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
        private static readonly MethodInfo _appendMethodInfo = typeof(StringBuilder).GetMethod("Append", new[] { typeof(string) });
        private static readonly MethodInfo _toStringMethodInfo = typeof(object).GetMethod("ToString", new Type[] { });


        private static readonly MethodInfo _trueBooleanCoercionMethodInfo = typeof(BooleanCoercion).GetMethod("IsTrue", new[] { typeof(object) });
        private static readonly MethodInfo _falseBooleanCoercionMethodInfo = typeof(BooleanCoercion).GetMethod("IsFalse", new[] { typeof(object) });
        private static readonly ConstructorInfo _listConstructorInfo = typeof(List<object>).GetConstructor(new[] { typeof(IEnumerable<object>) });
        private static readonly MethodInfo _integerRangeMethodInfo = typeof(IntegerRange).GetMethod("Range", new[] { typeof(int), typeof(int) });


        private static readonly MethodInfo _additionMethodInfo = typeof(Operators).GetMethod("Addition", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _subtractionMethodInfo = typeof(Operators).GetMethod("Subtraction", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _multiplicationMethodInfo = typeof(Operators).GetMethod("Multiplication", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _divisionMethodInfo = typeof(Operators).GetMethod("Division", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _moduloMethodInfo = typeof(Operators).GetMethod("Modulo", new[] { typeof(object), typeof(object) });

        private static readonly MethodInfo _andMethodInfo = typeof(Operators).GetMethod("And", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _orMethodInfo = typeof(Operators).GetMethod("Or", new[] { typeof(object), typeof(object) });

        private static readonly MethodInfo _lessThanMethodInfo = typeof(Comparators).GetMethod("LessThan", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _lessThanOrEqualMethodInfo = typeof(Comparators).GetMethod("LessThanOrEqual", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _greaterThanMethodInfo = typeof(Comparators).GetMethod("GreaterThan", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _greaterThanOrEqualMethodInfo = typeof(Comparators).GetMethod("GreaterThanOrEqual", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _equalMethodInfo = typeof(Comparators).GetMethod("Equal", new[] { typeof(object), typeof(object) });
        private static readonly MethodInfo _notEqualMethodInfo = typeof(Comparators).GetMethod("NotEqual", new[] { typeof(object), typeof(object) });



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
        public Expression BuildExpressionTree(ASTprocess ast, IDictionary<string, object> context)
        {
            if (ast == null)
                throw new ArgumentNullException("ast");

            if (!(ast is ASTprocess))
                throw new ArgumentOutOfRangeException("ast");

            List<Expression> expressions = new List<Expression>();

            if (context != null)
                expressions.AddRange(InitialiseEnvironment(context));

            expressions.AddRange(GetBlockExpressions(ast));

            if (!expressions.Any())
                expressions.Add(Expression.Empty());

            return Expression.Block(_locals.Values, expressions);
        }

        private IEnumerable<Expression> InitialiseEnvironment(IDictionary<string, object> context)
        {
            return context.Select(x => Expression.Assign(
                    GetLocal(x.Key),
                    Expression.Constant(x.Value, typeof(object)))
            );
        }


        private IDictionary<string, ParameterExpression> _locals = new Dictionary<string, ParameterExpression>(StringComparer.OrdinalIgnoreCase);

        public ParameterExpression GetLocal(string name)
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
                if (child is ASTText || child is ASTEscape)
                    expressions.Add(Text(child));
                else if (child is ASTReference)
                    expressions.Add(Reference(child, true));
                else if (child is ASTIfStatement)
                    expressions.Add(IfStatement(child));
                else if (child is ASTSetDirective)
                    expressions.Add(Set(child));
                else if (child is ASTEscapedDirective)
                    expressions.Add(Output(Escaped(child)));
                else if (child is ASTDirective)
                    expressions.Add(Directive(child));
                else if (child is ASTComment)
                    continue;
                else
                    throw new NotSupportedException("Node type not supported in a block: " + child.GetType().Name);

            }

            return expressions;
        }


        private static IDictionary<string, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<string, DirectiveExpressionBuilder>(StringComparer.OrdinalIgnoreCase)
        {
            {"foreach", new ForeachDirectiveExpressionBuilder()}
        };

        private Expression Directive(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var directive = node as ASTDirective;
            if (directive == null)
                throw new ArgumentOutOfRangeException("node");

            if (directive.Directive == null)
                return Text(node);

            DirectiveExpressionBuilder builder;

            if (_directiveHandlers.TryGetValue(directive.DirectiveName, out builder))
                return builder.Build(directive, this);
            else
                throw new NotSupportedException(String.Format("Unable to handle directive type '{0}'", directive.DirectiveName));
        }

        private Expression Escaped(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTEscapedDirective))
                throw new ArgumentOutOfRangeException("node");

            return Expression.Constant(node.Literal);
        }

        public Expression Block(INode node)
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





        public Expression Reference(INode node, bool renderable)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTReference))
                throw new ArgumentOutOfRangeException("node");

            //Remove $ or $! prefix to get just the variable name

            var metaData = new IronVelocity.INodeExtensions.ASTReferenceMetaData((ASTReference)node);

            Expression expr;
            if (metaData.Type == INodeExtensions.ASTReferenceMetaData.ReferenceType.Runt)
                expr = Expression.Constant(metaData.RootString);
            else
            {
                expr = GetLocal(metaData.RootString);

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
            }

            //If we're not rendering, we can return here
            if (!renderable)
                return expr;

            //If however we're rendering, we need to do funky stuff to the output, including rendering prefixes, suffixes etc.
            if (metaData.Escaped)
            {
                return Output(Expression.Condition(
                    Expression.NotEqual(expr, Expression.Constant(null, expr.Type)),
                    Expression.Constant(metaData.EscapePrefix + metaData.NullString),
                    Expression.Constant(metaData.EscapePrefix + "\\" + metaData.NullString)
                ));
            }
            else
            {
                var evaulatedResult = Expression.Parameter(expr.Type, "evaulatedResult");
                return Expression.Block(
                    new[] { evaulatedResult },
                    Expression.Assign(evaulatedResult, expr),
                    Expression.Condition(
                        Expression.NotEqual(evaulatedResult, Expression.Constant(null, evaulatedResult.Type)),
                        Expression.Block(
                            Output(Expression.Constant(metaData.EscapePrefix + metaData.MoreString)),
                            Output(evaulatedResult)
                        ),
                        Output(Expression.Constant(metaData.EscapePrefix + metaData.EscapePrefix + metaData.MoreString + metaData.NullString))
                    )
                );
            }
        }



        private Expression Method(INode node, Expression parent)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTMethod))
                throw new ArgumentOutOfRangeException("node");

            var methodName = node.FirstToken.Image;

            var arguments = new Expression[node.ChildrenCount];
            //First argument is the object to invoke the expression on
            arguments[0] = parent;

            //Subsequent arguments are the parameters
            for (int i = 1; i < node.ChildrenCount; i++)
            {
                arguments[i] = Statement(node.GetChild(i));
            }

            return Expression.Dynamic(
                new VelocityInvokeMemberBinder(methodName, new CallInfo(0)),
                typeof(object),
                arguments
            );
        }

        private static Expression Parameter(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTParameters))
                throw new ArgumentOutOfRangeException("node");

            throw new NotImplementedException();
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
                condition = Expression.Call(_trueBooleanCoercionMethodInfo, condition);

            return falseContent != null
                ? Expression.IfThenElse(condition, trueContent, falseContent)
                : Expression.IfThen(condition, trueContent);

        }


        public Expression Statement(INode node)
        {
            //Literal
            if (node is ASTTrue)
                return TrueExpression;
            else if (node is ASTFalse)
                return FalseExpression;
            else if (node is ASTNumberLiteral)
                return NumberLiteral(node);
            else if (node is ASTStringLiteral)
                return StringLiteral(node);
            //Logical
            else if (node is ASTAndNode)
                return And(node);
            else if (node is ASTOrNode)
                return Or(node);
            else if (node is ASTNotNode)
                return Not(node);
            //Comparison
            else if (node is ASTLTNode)
                return LessThan(node);
            else if (node is ASTLENode)
                return LessThanOrEqual(node);
            else if (node is ASTGTNode)
                return GreaterThan(node);
            else if (node is ASTGENode)
                return GreaterThanOrEqual(node);
            else if (node is ASTEQNode)
                return Equal(node);
            else if (node is ASTNENode)
                return NotEqual(node);
            //Mathematical Operations
            else if (node is ASTAddNode)
                return Addition(node);
            else if (node is ASTSubtractNode)
                return Subtraction(node);
            else if (node is ASTMulNode)
                return Multiplication(node);
            else if (node is ASTDivNode)
                return Division(node);
            else if (node is ASTModNode)
                return Modulo(node);
            //Code
            else if (node is ASTAssignment)
                return Assign(node);
            else if (node is ASTReference)
                return Reference(node, false);
            else if (node is ASTObjectArray)
                return Array(node);
            else if (node is ASTIntegerRange)
                return IntegerRange(node);
            else if (node is ASTExpression)
                return Expr(node);
            else
                throw new NotSupportedException("Node type not supported in an expression: " + node.GetType().Name);
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
            return Statement(child);
        }

        private Expression Array(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTObjectArray))
                throw new ArgumentOutOfRangeException("node");

            var elements = node.GetChildren()
                .Select(Statement);

            return Expression.New(_listConstructorInfo, Expression.NewArrayInit(typeof(object), elements));
        }

        private Expression IntegerRange(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTIntegerRange))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

            return Expression.Call(null, _integerRangeMethodInfo,
                VelocityExpressions.ConvertIfNeeded(left, typeof(object)),
                VelocityExpressions.ConvertIfNeeded(right, typeof(object))
                );
        }

        private Expression StringLiteral(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTStringLiteral))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount > 0)
                throw new NotImplementedException("Not supporting interpolated strings yet");

            return Expression.Constant(node.Literal.Substring(1, node.Literal.Length - 2));
        }

        private Expression Text(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var text = NodeUtils.tokenLiteral(node.FirstToken);
            if (text != node.Literal)
            {
            }
            return Output(Expression.Constant(text));
        }

        private static Expression Output(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (expression.Type != typeof(string))
                expression = Expression.Call(expression, _toStringMethodInfo);

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
            if (node is ASTTrue)
                return TrueExpression;
            else if (node is ASTFalse)
                return FalseExpression;
            else if (node is ASTNumberLiteral)
                return NumberLiteral(node);
            else if (node is ASTStringLiteral)
                return StringLiteral(node);
            else if (node is ASTReference)
                return Reference(node, false);
            else if (node is ASTExpression)
                return Expr(node);
            else if (node is ASTNotNode)
                return Not(node);
            else
                throw new NotSupportedException("Node type not supported in an operand: " + node.GetType().Name);
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

            if (left.NodeType == ExpressionType.Dynamic)
                throw new NotSupportedException("Don't currently support assignment to dotted properties");

            /* One of the nuances of velocity is that if the right evaluates to null,
             * Thus we can't simply return an assignment expression.
             * The resulting expression looks as follows:P
             *     .set $tempResult = right;
             *     .if ($tempResult != null){
             *         Assign(left, right)
             *     }
             */
            var tempResult = Expression.Parameter(right.Type);
            return Expression.Block(new[] { tempResult },
                //Store the result of the right hand side in to a temporary variable
                Expression.Assign(tempResult, right),
                Expression.IfThen(
                //If the temporary variable is not equal to null
                    Expression.NotEqual(tempResult, Expression.Constant(null, right.Type)),
                //Make the assignment
                    Expression.Assign(left, tempResult)
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
            left = VelocityExpressions.ConvertIfNeeded(left, typeof(object));
            right = VelocityExpressions.ConvertIfNeeded(right, typeof(object));

            return generator(left, right, implementation);
        }

        private static Expression BinaryLogicalExpression(Func<Expression, Expression, bool, MethodInfo, Expression> generator, Expression left, Expression right, MethodInfo implementation)
        {
            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            left = VelocityExpressions.ConvertIfNeeded(left, typeof(object));
            right = VelocityExpressions.ConvertIfNeeded(right, typeof(object));

            return generator(left, right, false, implementation);
        }

        #endregion

        #region Logical Operators

        /// <summary>
        /// Builds an equals expression from an ASTLTNode
        /// </summary>
        /// <param name="node">The ASTLTNode to build the less than Expression from</param>
        /// <returns>An expression representing the less than operation</returns>
        private Expression LessThan(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTLTNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryLogicalExpression(Expression.LessThan, left, right, _lessThanMethodInfo);
        }


        private Expression LessThanOrEqual(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTLENode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryLogicalExpression(Expression.LessThanOrEqual, left, right, _lessThanOrEqualMethodInfo);
        }

        /// <summary>
        /// Builds a greater than expression from an ASTGTNode
        /// </summary>
        /// <param name="node">The ASTGTNode to build the greater than Expression from</param>
        /// <returns>An expression representing the greater than operation</returns>
        private Expression GreaterThan(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTGTNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryLogicalExpression(Expression.GreaterThan, left, right, _greaterThanMethodInfo);
        }
        /// <summary>
        /// Builds a greater than or equal to expression from an ASTGENode
        /// </summary>
        /// <param name="node">The ASTGENode to build the equals Expression from</param>
        /// <returns>An expression representing the greater than or equal to operation</returns>
        private Expression GreaterThanOrEqual(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTGENode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryLogicalExpression(Expression.GreaterThanOrEqual, left, right, _greaterThanOrEqualMethodInfo);
        }

        /// <summary>
        /// Builds an equals expression from an ASTEQNode
        /// </summary>
        /// <param name="node">The ASTEQNode to build the equals Expression from</param>
        /// <returns>An expression representing the equals operation</returns>
        private Expression Equal(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTEQNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryLogicalExpression(Expression.Equal, left, right, _equalMethodInfo);
        }

        /// <summary>
        /// Builds a not equal expression from an ASTNENode
        /// </summary>
        /// <param name="node">The ASTNENode to build the not equal Expression from</param>
        /// <returns>An expression representing the not equal  operation</returns>
        private Expression NotEqual(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTNENode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryLogicalExpression(Expression.NotEqual, left, right, _notEqualMethodInfo);
        }

        #endregion

        #region Logical

        private Expression Not(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTNotNode))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException("node");


            var expr = VelocityExpressions.ConvertIfNeeded(Operand(node.GetChild(0)), typeof(object));

            return Expression.Not(expr, _falseBooleanCoercionMethodInfo);
        }

        private Expression And(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTAndNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryMathematicalExpression(Expression.And, left, right, _andMethodInfo);
        }

        private Expression Or(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTOrNode))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);
            return BinaryMathematicalExpression(Expression.Or, left, right, _orMethodInfo);
        }


        #endregion
    }
}
