using IronVelocity.Binders;
using NVelocity.Exception;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using IronVelocity.Compilation.AST;

namespace IronVelocity.Compilation
{
    public static class NVelocityExpressions
    {
        public static Expression IfDirective(INode node, VelocityExpressionBuilder builder)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTIfStatement))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount < 2)
                throw new ArgumentOutOfRangeException("node", "Expected at least 2 children");

            if (builder == null)
                throw new ArgumentOutOfRangeException("builder");


            var condition = new CoerceToBooleanExpression(Expr(node.GetChild(0)));
            var trueContent = new RenderedBlock(builder.GetBlockExpressions(node.GetChild(1)), builder);
            Expression falseContent = Expression.Default(typeof(void));

            //Build the false expression recursively from the bottom up
            for (int i = node.ChildrenCount - 1; i > 1; i--)
            {
                var child = node.GetChild(i);
                if (child is ASTElseStatement)
                {
                    if (i != node.ChildrenCount - 1)
                        throw new InvalidOperationException("ASTElseStatement may only be the last child of an ASTIfStatement");

                    if (child.ChildrenCount != 1)
                        throw new InvalidOperationException("Expected ASTElseStatement to only have 1 child");

                    falseContent = new RenderedBlock(builder.GetBlockExpressions(child.GetChild(0)), builder);
                }
                else if (child is ASTElseIfStatement)
                {
                    var innerCondition = new CoerceToBooleanExpression(Expr(child.GetChild(0)));
                    var innerContent = new RenderedBlock(builder.GetBlockExpressions(child.GetChild(1)), builder);

                    falseContent = Expression.IfThenElse(innerCondition, innerContent, falseContent);
                }
                else

                    throw new InvalidOperationException("Expected: ASTElseStatement, Actual: " + child.GetType().Name);
            }

            return Expression.IfThenElse(condition, trueContent, falseContent);
        }

        public static SetDirective Set(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.Type != ParserTreeConstants.SET_DIRECTIVE)
                throw new ArgumentOutOfRangeException("node");

            return Assignment(node.GetChild(0).GetChild(0));
        }

        public static SetDirective Assignment(INode node)
        {
            Expression left, right;
            GetBinaryExpressionOperands<ASTAssignment>(node, out left, out right);

            return new SetDirective(left, right, new SymbolInformation(node));
        }

        public static IntegerRangeExpression IntegerRange(INode node)
        {
            Expression left, right;
            GetBinaryExpressionOperands<ASTIntegerRange>(node, out left, out right);

            return new IntegerRangeExpression(left, right, new SymbolInformation(node));
        }

        public static ObjectArrayExpression ObjectArray(INode node)
        {
            if (!(node is ASTObjectArray))
                throw new ArgumentOutOfRangeException("node");

            var expressions = new Expression[node.ChildrenCount];
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                expressions[i] = Operand(node.GetChild(i));
            }

            return new ObjectArrayExpression(new SymbolInformation(node), expressions);
        }


        #region Comparison Expressions

        public static ComparisonExpression LessThan(INode node)
        {
            return Comparison<ASTLTNode>(node, ComparisonOperation.LessThan);
        }

        public static ComparisonExpression GreaterThan(INode node)
        {
            return Comparison<ASTGTNode>(node, ComparisonOperation.GreaterThan);
        }

        public static ComparisonExpression LessThanOrEqual(INode node)
        {
            return Comparison<ASTLENode>(node, ComparisonOperation.LessThanOrEqual);
        }

        public static ComparisonExpression GreaterThanOrEqual(INode node)
        {
            return Comparison<ASTGENode>(node, ComparisonOperation.GreaterThanOrEqual);
        }

        public static ComparisonExpression Equal(INode node)
        {
            return Comparison<ASTEQNode>(node, ComparisonOperation.Equal);
        }

        public static ComparisonExpression NotEqual(INode node)
        {
            return Comparison<ASTNENode>(node, ComparisonOperation.NotEqual);
        }

        private static ComparisonExpression Comparison<T>(INode node, ComparisonOperation operation)
            where T : INode
        {
            Expression left, right;
            GetBinaryExpressionOperands<T>(node, out left, out right);

            return new ComparisonExpression(left, right, new SymbolInformation(node), operation);
        }
        #endregion

        #region Boolean Expressions

        private static Expression Not(INode node)
        {
            if (!(node is ASTNotNode))
                throw new ArgumentOutOfRangeException("node");

            var operand = Operand(node.GetChild(0));
            var expression = VelocityExpressions.CoerceToBoolean(operand);

            return Expression.Not(expression);
        }

        private static Expression And(INode node)
        {
            return BinaryBooleanExpression<ASTAndNode>(Expression.AndAlso, node);
        }

        private static Expression Or(INode node)
        {
            return BinaryBooleanExpression<ASTOrNode>(Expression.OrElse, node);
        }

        private static Expression BinaryBooleanExpression<T>(Func<Expression, Expression, Expression> generator, INode node)
            where T : INode
        {
            Expression left, right;
            GetBinaryExpressionOperands<T>(node, out left, out right);

            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            left = VelocityExpressions.CoerceToBoolean(left);
            right = VelocityExpressions.CoerceToBoolean(right);

            return generator(left, right);
        }

        #endregion

        #region Mathematical Expressions

        public static Expression Add(INode node)
        {
            return Mathematical<ASTAddNode>(node, MathematicalOperation.Add);
        }

        public static Expression Subtract(INode node)
        {
            return Mathematical<ASTSubtractNode>(node, MathematicalOperation.Subtract);
        }

        public static Expression Multiply(INode node)
        {
            return Mathematical<ASTMulNode>(node, MathematicalOperation.Multiply);
        }

        public static Expression Divide(INode node)
        {
            return Mathematical<ASTDivNode>(node, MathematicalOperation.Divide);
        }


        public static Expression Modulo(INode node)
        {
            return Mathematical<ASTModNode>(node, MathematicalOperation.Modulo);
        }

        private static MathematicalExpression Mathematical<T>(INode node, MathematicalOperation operation)
            where T : INode
        {
            Expression left, right;
            GetBinaryExpressionOperands<T>(node, out left, out right);

            return new MathematicalExpression(left, right, new SymbolInformation(node), operation);
        }
        #endregion



        public static ReferenceExpression Reference(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var refNode = node as ASTReference;
            if (refNode == null)
                throw new ArgumentOutOfRangeException("node");

            var metadata = new ASTReferenceMetadata(refNode);
            Expression value = new VariableExpression(metadata.RootString);

            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
                if (child is ASTIdentifier)
                    value = Property(child, value);
                else if (child is ASTMethod)
                    value = Method(child, value);
                else
                    throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);
            }

            return new ReferenceExpression(metadata, value);

        }

        public static MethodInvocationExpression Method(INode node, Expression target)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (target == null)
                throw new ArgumentNullException("target");


            var arguments = new Expression[node.ChildrenCount - 1];
            //Subsequent arguments are the parameters
            for (int i = 1; i < node.ChildrenCount; i++)
            {
                arguments[i - 1] = (Operand(node.GetChild(i)));
            }

            return new MethodInvocationExpression(target, node.FirstToken.Image, arguments, new SymbolInformation(node));
        }


        public static Expression Property(INode node, Expression target)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (target == null)
                throw new ArgumentNullException("target");

            return new PropertyAccessExpression(target, node.Literal, new SymbolInformation(node));
        }

        public static Expression NVelocityString(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTStringLiteral))
                throw new ArgumentOutOfRangeException("node");

            var value = node.Literal.Substring(1, node.Literal.Length - 2);

            var isDoubleQuoted = node.Literal.StartsWith("\"", StringComparison.Ordinal);
            var stringType = isDoubleQuoted
                ? DetermineStringType(value)
                : VelocityStringType.Constant;

            switch (stringType)
            {
                case VelocityStringType.Constant:
                    return Expression.Constant(value);
                case VelocityStringType.Dictionary:
                    return new DictionaryStringExpression(value);
                case VelocityStringType.Interpolated:
                    return InterpolatedString(value);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static InterpolatedStringExpression InterpolatedString(string value)
        {
            var parser = new NVelocity.Runtime.RuntimeInstance().CreateNewParser();
            using (var reader = new System.IO.StringReader(value))
            {
                SimpleNode ast;
                try
                {
                    ast = parser.Parse(reader, null);
                }
                catch (ParseErrorException)
                {
                    ast = null;
                }

                //If we fail to parse, the ast returned will be null, so just return our normal string
                if (ast == null)
                    return new InterpolatedStringExpression(Expression.Constant(value));

                var parts = new VelocityExpressionBuilder(null)
                    .GetBlockExpressions(ast)
                    .Where(x => x.Type != typeof(void))
                    .ToArray();

                return new InterpolatedStringExpression(parts);
            }
        }

        public static VelocityStringType DetermineStringType(string value)
        {
            if (value == null)
                return VelocityStringType.Constant;
            if (value.StartsWith("%{", StringComparison.Ordinal) && value.EndsWith("}", StringComparison.Ordinal))
                return VelocityStringType.Dictionary;
            if (value.IndexOfAny(new[] { '$', '#' }) != -1)
                return VelocityStringType.Interpolated;
            else
                return VelocityStringType.Constant;
        }

        public static Expression Expr(INode node)
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

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="Unavoidable")]
        public static Expression Operand(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            switch (node.Type)
            {
                case ParserTreeConstants.TRUE:
                    return Constants.True;
                case ParserTreeConstants.FALSE:
                    return Constants.False;
                case ParserTreeConstants.NUMBER_LITERAL:
                    return Expression.Constant(int.Parse(node.Literal, CultureInfo.InvariantCulture)); ;
                case ParserTreeConstants.STRING_LITERAL:
                    return NVelocityString(node);

                //Boolean
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
                    return Add(node);
                case ParserTreeConstants.SUBTRACT_NODE:
                    return Subtract(node);
                case ParserTreeConstants.MUL_NODE:
                    return Multiply(node);
                case ParserTreeConstants.DIV_NODE:
                    return Divide(node);
                case ParserTreeConstants.MOD_NODE:
                    return Modulo(node);

                //Code
                case ParserTreeConstants.ASSIGNMENT:
                    return Assignment(node);
                case ParserTreeConstants.REFERENCE:
                    return Reference(node);
                case ParserTreeConstants.OBJECT_ARRAY:
                    return ObjectArray(node);
                case ParserTreeConstants.INTEGER_RANGE:
                    return IntegerRange(node);
                case ParserTreeConstants.EXPRESSION:
                    return Expr(node);

                default:
                    throw new NotSupportedException("Node type not supported in an expression: " + node.GetType().Name);
            }
        }

        private static void GetBinaryExpressionOperands<T>(INode node, out Expression left, out Expression right)
            where T : INode
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is T))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount != 2)
                throw new ArgumentOutOfRangeException("node", "Expected exactly two children for a binary expression");

            left = Operand(node.GetChild(0));
            right = Operand(node.GetChild(1));
        }


        public enum VelocityStringType
        {
            Constant,
            Dictionary,
            Interpolated
        }
    }
}
