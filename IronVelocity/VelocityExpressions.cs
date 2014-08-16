using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public abstract partial class VelocityExpression : Expression
    {
        private static readonly Expression TrueExpression = Expression.Constant(true);
        private static readonly Expression FalseExpression = Expression.Constant(false);


        public static SetDirective Set(INode node)
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

        public static BinaryLogicalExpression LessThan(INode node)
        {
            return BinaryLogical<ASTLTNode>(node, LogicalOperation.LessThan);
        }

        public static BinaryLogicalExpression GreaterThan(INode node)
        {
            return BinaryLogical<ASTGTNode>(node, LogicalOperation.GreaterThan);
        }

        public static BinaryLogicalExpression LessThanOrEqual(INode node)
        {
            return BinaryLogical<ASTLENode>(node, LogicalOperation.LessThanOrEqual);
        }

        public static BinaryLogicalExpression GreaterThanOrEqual(INode node)
        {
            return BinaryLogical<ASTGENode>(node, LogicalOperation.GreaterThanOrEqual);
        }

        public static BinaryLogicalExpression Equal(INode node)
        {
            return BinaryLogical<ASTEQNode>(node, LogicalOperation.Equal);
        }

        public static BinaryLogicalExpression NotEqual(INode node)
        {
            return BinaryLogical<ASTNENode>(node, LogicalOperation.NotEqual);
        }

        private static BinaryLogicalExpression BinaryLogical<T>(INode node, LogicalOperation operation)
            where T : INode
        {
            Expression left, right;
            GetBinaryExpressionOperands<T>(node, out left, out right);

            return new BinaryLogicalExpression(left, right, new SymbolInformation(node), operation);
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
            return BinaryMathematical<ASTAddNode>(node, MathematicalOperation.Add);
        }

        public static Expression Subtract(INode node)
        {
            return BinaryMathematical<ASTSubtractNode>(node, MathematicalOperation.Subtract);
        }

        public static Expression Multiply(INode node)
        {
            return BinaryMathematical<ASTMulNode>(node, MathematicalOperation.Multiply);
        }

        public static Expression Divide(INode node)
        {
            return BinaryMathematical<ASTDivNode>(node, MathematicalOperation.Divide);
        }


        public static Expression Modulo(INode node)
        {
            return BinaryMathematical<ASTModNode>(node, MathematicalOperation.Modulo);
        }

        private static BinaryMathematicalExpression BinaryMathematical<T>(INode node, MathematicalOperation operation)
            where T : INode
        {
            Expression left, right;
            GetBinaryExpressionOperands<T>(node, out left, out right);

            return new BinaryMathematicalExpression(left, right, new SymbolInformation(node), operation);
        }
        #endregion



        public static ReferenceExpression Reference(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var refNode = node as ASTReference;
            if (refNode == null)
                throw new ArgumentOutOfRangeException("node");

            //return new ReferenceExpression(node);

            var metadata = new ASTReferenceMetadata(refNode);

            var baseVariable = new VariableExpression(metadata.RootString);

            var additional = new List<Expression>(node.ChildrenCount);

            Expression soFar = baseVariable;
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
                if (child is ASTIdentifier)
                    soFar = Property(child, soFar);
                else if (child is ASTMethod)
                    soFar = Method(child, soFar);
                else
                    throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);

                additional.Add(soFar);
            }

            return new ReferenceExpression(metadata, baseVariable, additional);

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
                    return new InterpolatedStringExpression(value);
                default:
                    throw new InvalidOperationException();
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

        public static Expression Operand(INode node)
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
                    return Set(node);
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
                throw new ArgumentOutOfRangeException("Expected exactly two children for a binary expression");

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
