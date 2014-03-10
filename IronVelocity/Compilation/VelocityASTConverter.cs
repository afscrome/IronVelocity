using IronVelocity.Binders;
using IronVelocity.Compilation.Directives;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation
{
    [CLSCompliant(false)]
    public class VelocityASTConverter
    {
        private static readonly Expression TrueExpression = Expression.Constant(true);
        private static readonly Expression FalseExpression = Expression.Constant(false);

        private IDictionary<string, VelocityGetMemberBinder> _binders = new Dictionary<string, VelocityGetMemberBinder>(StringComparer.OrdinalIgnoreCase);
        private IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers;
        private IScope _scope = new BaseScope(Constants.InputParameter);
        private SymbolDocumentInfo _symbolDocument;
        private static ParameterExpression _evaulatedResult = Expression.Parameter(typeof(object), "tempEvaulatedResult");

        public VelocityASTConverter(IDictionary<Type, DirectiveExpressionBuilder> directiveHandlers)
        {
            _directiveHandlers = directiveHandlers;
        }


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
        public Expression BuildExpressionTree(ASTprocess ast, string fileName)
        {
            if (ast == null)
                throw new ArgumentNullException("ast");

            if (!(ast is ASTprocess))
                throw new ArgumentOutOfRangeException("ast");

            List<Expression> expressions = new List<Expression>();
            _symbolDocument = Expression.SymbolDocument(fileName);


            expressions.AddRange(GetBlockExpressions(ast, true));

            if (!expressions.Any())
                return Expression.Default(typeof(void));

            return Expression.Block(new[] { _evaulatedResult }, expressions);
        }

        public Expression GetVariable(string name)
        {
            return _scope.GetVariable(name);
        }



        public Expression DebugInfo(INode node, Expression expression)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            return Expression.Block(
                Expression.DebugInfo(_symbolDocument, node.FirstToken.BeginLine + 1, node.FirstToken.BeginColumn, node.LastToken.EndLine + 1, node.LastToken.EndColumn),
                expression
                //, Expression.ClearDebugInfo(_symbolDocument)
            );
        }

        public IEnumerable<Expression> GetBlockExpressions(INode node, bool output)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            //ASTprocess is a special case for the root, otherwise it behaves exactly like ASTBlock
            if (!(node is ASTBlock || node is ASTprocess))
                throw new ArgumentOutOfRangeException("node");

            var expressions = new List<Expression>();


            foreach (var child in node.GetChildren())
            {
                Expression expr;
                switch (child.Type)
                {
                    case ParserTreeConstants.TEXT:
                    case ParserTreeConstants.ESCAPE:
                        expr = Text(child);
                        if (output)
                            expr = Output(expr);
                        break;
                    case ParserTreeConstants.REFERENCE:
                        expr = Reference(child, output);
                        if (output)
                            expr = Output(expr);
                        break;
                    case ParserTreeConstants.IF_STATEMENT:
                        expr = IfStatement(child);
                        break;
                    case ParserTreeConstants.SET_DIRECTIVE:
                        expr = Set(child);
                        break;
                    case ParserTreeConstants.ESCAPED_DIRECTIVE:
                        expr = Escaped(child);
                        if (output)
                            expr = Output(expr);
                        break;
                    case ParserTreeConstants.DIRECTIVE:
                        expr = Directive(child);
                        if (output)
                            expr = Output(expr);
                        break;
                    case ParserTreeConstants.COMMENT:
                        continue;

                    default:
                        throw new NotSupportedException("Node type not supported in a block: " + child.GetType().Name);
                }


                expressions.Add(expr);
            }

            return expressions;
        }



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

            if (_directiveHandlers.TryGetValue(directive.Directive.GetType(), out builder))
                return builder.Build(directive, this);
            else
                throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "Unable to handle directive type '{0}'", directive.DirectiveName));
        }

        private Expression Escaped(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTEscapedDirective))
                throw new ArgumentOutOfRangeException("node");

            return DebugInfo(node, Expression.Constant(node.Literal));
        }

        public Expression Block(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTBlock))
                throw new ArgumentOutOfRangeException("node");

            var children = GetBlockExpressions(node, true);

            Expression expr;
            if (children.Any())
                expr = Expression.Block(children);
            else
                expr = Expression.Empty();

            return DebugInfo(node, expr);
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

            var refNode = node as ASTReference;
            if (refNode == null)
                throw new ArgumentOutOfRangeException("node");

            //Remove $ or $! prefix to get just the variable name

            var metaData = new ASTReferenceMetadata(refNode);

            Expression expr;
            if (metaData.RefType == ASTReferenceMetadata.ReferenceType.Runt)
                expr = Expression.Constant(metaData.RootString);
            else
            {
                expr = GetVariable(metaData.RootString);

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
                return Expression.Condition(
                    Expression.NotEqual(expr, Expression.Constant(null, expr.Type)),
                    Expression.Constant(metaData.EscapePrefix + metaData.NullString),
                    Expression.Constant(metaData.EscapePrefix + "\\" + metaData.NullString)
                );
            }
            else
            {
                //TODO: this fails if return type is void
                return Expression.Block(
                    Expression.Assign(_evaulatedResult, expr),
                    Expression.Condition(
                        Expression.NotEqual(_evaulatedResult, Expression.Constant(null, _evaulatedResult.Type)),
                        Expression.Call(
                            MethodHelpers.StringConcatMethodInfo,
                            Expression.Convert(Expression.Constant(metaData.EscapePrefix + metaData.MoreString), typeof(object)),
                            VelocityExpressions.ConvertIfNeeded(_evaulatedResult, typeof(object))
                        ),
                        Expression.Constant(metaData.EscapePrefix + metaData.EscapePrefix + metaData.MoreString + metaData.NullString)
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
                arguments[i] = Operand(node.GetChild(i));
            }

            return DebugInfo(
                node,
                Expression.Dynamic(
                    new VelocityInvokeMemberBinder(methodName, new CallInfo(0)),
                    typeof(object),
                    arguments
                )
            );
        }

        private Expression Identifier(INode node, Expression parent)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTIdentifier))
                throw new ArgumentOutOfRangeException("node");

            var propertyName = node.Literal;
            return DebugInfo(
                node,
                Expression.Dynamic(
                    GetGetMemberBinder(propertyName),
                    typeof(object),
                    parent
                )
            );
        }


        private static Expression NumberLiteral(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTNumberLiteral))
                throw new ArgumentOutOfRangeException("node");

            return Expression.Constant(int.Parse(node.Literal, CultureInfo.InvariantCulture));
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

                    falseContent = If(child, innerCondition, innerContent, falseContent);
                }
                else

                    throw new InvalidOperationException("Expected: ASTElseStatement, Actual: " + child.GetType().Name);
            }
            return If(node, condition, trueContent, falseContent);
        }

        /// <summary>
        /// Helper to build an If or an IfElse statement based on whether we have the falseContent block is not null
        /// </summary>
        /// <returns></returns>
        private Expression If(INode node, Expression condition, Expression trueContent, Expression falseContent)
        {
            if (condition.Type != typeof(bool))
                condition = Expression.Call(MethodHelpers.TrueBooleanCoercionMethodInfo, VelocityExpressions.BoxIfNeeded(condition));

            var expr = falseContent != null
                ? Expression.IfThenElse(condition, trueContent, falseContent)
                : Expression.IfThen(condition, trueContent);

            return DebugInfo(node, expr);
        }


        public Expression Operand(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            switch (node.Type)
            {
                case ParserTreeConstants.TRUE:
                    return TrueExpression;
                case ParserTreeConstants.FALSE:
                    return FalseExpression;
                case ParserTreeConstants.NUMBER_LITERAL:
                    return NumberLiteral(node);

                case ParserTreeConstants.STRING_LITERAL:
                    return StringLiteral(node);
                case ParserTreeConstants.AND_NODE:
                    return And(node);
                case ParserTreeConstants.OR_NODE:
                    return Or(node);
                case ParserTreeConstants.NOT_NODE:
                    return Not(node);

                //Comparison
                case ParserTreeConstants.LT_NODE:
                    return LessThan(node);
                case ParserTreeConstants.LE_NODE:
                    return LessThanOrEqual(node);
                case ParserTreeConstants.GT_NODE:
                    return GreaterThan(node);
                case ParserTreeConstants.GE_NODE:
                    return GreaterThanOrEqual(node);
                case ParserTreeConstants.EQ_NODE:
                    return Equal(node);
                case ParserTreeConstants.NE_NODE:
                    return NotEqual(node);
                //Mathematical Operations
                case ParserTreeConstants.ADD_NODE:
                    return Addition(node);
                case ParserTreeConstants.SUBTRACT_NODE:
                    return Subtraction(node);
                case ParserTreeConstants.MUL_NODE:
                    return Multiplication(node);
                case ParserTreeConstants.DIV_NODE:
                    return Division(node);
                case ParserTreeConstants.MOD_NODE:
                    return Modulo(node);
                //Code
                case ParserTreeConstants.ASSIGNMENT:
                    return Assign(node);
                case ParserTreeConstants.REFERENCE:
                    return Reference(node, false);
                case ParserTreeConstants.OBJECT_ARRAY:
                    return Array(node);
                case ParserTreeConstants.INTEGER_RANGE:
                    return IntegerRange(node);
                case ParserTreeConstants.EXPRESSION:
                    return Expr(node);
                default:
                    throw new NotSupportedException("Node type not supported in an expression: " + node.GetType().Name);
            }
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
            return Operand(child);
        }

        private Expression Array(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTObjectArray))
                throw new ArgumentOutOfRangeException("node");

            var elements = node.GetChildren()
                .Select(Operand);

            return DebugInfo(node, Expression.New(MethodHelpers.ListConstructorInfo, Expression.NewArrayInit(typeof(object), elements)));
        }

        private Expression IntegerRange(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTIntegerRange))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

            return DebugInfo(
                node,
                    Expression.Call(null, MethodHelpers.IntegerRangeMethodInfo,
                    VelocityExpressions.ConvertIfNeeded(left, typeof(object)),
                    VelocityExpressions.ConvertIfNeeded(right, typeof(object))
                    )
                );
        }

        private Expression StringLiteral(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTStringLiteral))
                throw new ArgumentOutOfRangeException("node");

            var isDoubleQuoted = node.Literal.StartsWith("\"", StringComparison.Ordinal);
            var content = node.Literal.Substring(1, node.Literal.Length - 2);

            var stringType = isDoubleQuoted
                ? VelocityStrings.DetermineStringType(content)
                : VelocityStrings.StringType.Constant;

            Expression expr;

            switch (stringType)
            {
                case VelocityStrings.StringType.Constant:
                    expr = Expression.Constant(content);
                    break;

                case VelocityStrings.StringType.Dictionary:
                    expr = VelocityStrings.InterpolateDictionaryString(content, this);
                    break;
                case VelocityStrings.StringType.Interpolated:
                    expr = VelocityStrings.InterpolateString(content, this);
                    break;

                default:
                    throw new InvalidProgramException();
            }

            return DebugInfo(node, expr);
        }


        private Expression Text(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var text = NodeUtils.tokenLiteral(node.FirstToken);
            if (text != node.Literal)
            {
            }
            return DebugInfo(node, Expression.Constant(text));
        }

        private static Expression Output(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            //If the expression is of type void, we can't print anything, so just return the current expression
            if (expression.Type == typeof(void))
                return expression;

            if (expression.Type != typeof(string))
                expression = Expression.Call(expression, MethodHelpers.ToStringMethodInfo);


            return Expression.Call(Constants.OutputParameter, MethodHelpers.AppendMethodInfo, expression);
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

            if (left.NodeType == ExpressionType.Block)
            {
                var leftBlock = (BlockExpression)left;
                if (leftBlock.Expressions.Count == 2 && leftBlock.Expressions[0] is DebugInfoExpression)
                {
                    left = leftBlock.Expressions[1];
                }
            }

            Expression assignment = null;
            if (left.NodeType == ExpressionType.Dynamic)
            {
                var dynamic = (DynamicExpression)left;
                var getBinder = dynamic.Binder as VelocityGetMemberBinder;
                if (getBinder != null)
                {
                    assignment = Expression.Dynamic(
                        new VelocitySetMemberBinder(getBinder.Name),
                        typeof(object),
                        dynamic.Arguments[0],
                        right
                        );
                }
            }
            if (assignment == null)
            {
                /* One of the nuances of velocity is that if the right evaluates to null,
                 * Thus we can't simply return an assignment expression.
                 * The resulting expression looks as follows:P
                 *     .set $tempResult = right;
                 *     .if ($tempResult != null){
                 *         Assign(left, right)
                 *     }
                 */
                var tempResult = Expression.Parameter(right.Type);
                assignment = Expression.Block(new[] { tempResult },
                    //Store the result of the right hand side in to a temporary variable
                    Expression.Assign(tempResult, right),
                    Expression.IfThen(
                    //If the temporary variable is not equal to null
                        Expression.NotEqual(tempResult, Expression.Constant(null, right.Type)),
                    //Make the assignment
                        Expression.Assign(left, tempResult)
                    )
                );
            }

            return DebugInfo(node, assignment);
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

            return BinaryMathematicalExpression(Expression.Add, node, MethodHelpers.AdditionMethodInfo);
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

            return BinaryMathematicalExpression(Expression.Subtract, node, MethodHelpers.SubtractionMethodInfo);
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

            return BinaryMathematicalExpression(Expression.Multiply, node, MethodHelpers.MultiplicationMethodInfo);
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

            return BinaryMathematicalExpression(Expression.Divide, node, MethodHelpers.DivisionMethodInfo);
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

            return BinaryMathematicalExpression(Expression.Divide, node, MethodHelpers.ModuloMethodInfo);
        }


        private Expression BinaryMathematicalExpression(Func<Expression, Expression, MethodInfo, Expression> generator, INode node, MethodInfo implementation)
        {
            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            left = VelocityExpressions.ConvertIfNeeded(left, typeof(object));
            right = VelocityExpressions.ConvertIfNeeded(right, typeof(object));

            return DebugInfo(node, generator(left, right, implementation));
        }

        private Expression BinaryLogicalExpression(Func<Expression, Expression, bool, MethodInfo, Expression> generator, INode node, MethodInfo implementation)
        {
            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            left = VelocityExpressions.ConvertIfNeeded(left, typeof(object));
            right = VelocityExpressions.ConvertIfNeeded(right, typeof(object));

            return DebugInfo(node, generator(left, right, false, implementation));
        }

        #endregion

        #region Logical Comparators

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

            return BinaryLogicalExpression(Expression.LessThan, node, MethodHelpers.LessThanMethodInfo);
        }


        private Expression LessThanOrEqual(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTLENode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogicalExpression(Expression.LessThanOrEqual, node, MethodHelpers.LessThanOrEqualMethodInfo);
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

            return BinaryLogicalExpression(Expression.GreaterThan, node, MethodHelpers.GreaterThanMethodInfo);
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

            return BinaryLogicalExpression(Expression.GreaterThanOrEqual, node, MethodHelpers.GreaterThanOrEqualMethodInfo);
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

            return BinaryLogicalExpression(Expression.Equal, node, MethodHelpers.EqualMethodInfo);
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

            return BinaryLogicalExpression(Expression.NotEqual, node, MethodHelpers.NotEqualMethodInfo);
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

            return DebugInfo(node, Expression.Not(expr, MethodHelpers.FalseBooleanCoercionMethodInfo));
        }

        private Expression And(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTAndNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryMathematicalExpression(Expression.And, node, MethodHelpers.AndMethodInfo);
        }

        private Expression Or(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTOrNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryMathematicalExpression(Expression.Or, node, MethodHelpers.OrMethodInfo);
        }


        #endregion
    }
}
