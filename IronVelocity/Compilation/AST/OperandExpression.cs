using IronVelocity.Binders;
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
    /// <summary>
    /// TODO: I don't like this class - refactor.
    /// </summary>
    public static class ConversionHelpers
    {

        private static readonly Expression TrueExpression = Expression.Constant(true);
        private static readonly Expression FalseExpression = Expression.Constant(false);


        public static IReadOnlyCollection<Expression> GetBlockExpressions(INode node)
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
                        var content = NodeUtils.tokenLiteral(child.FirstToken);
                        expr = Expression.Constant(content);
                        break;
                    case ParserTreeConstants.ESCAPED_DIRECTIVE:
                        expr = Expression.Constant(child.Literal);
                        break;
                    case ParserTreeConstants.REFERENCE:
                        expr = new DynamicReference(child);
                        break;
                    case ParserTreeConstants.IF_STATEMENT:
                        expr = new IfStatement(child);
                        break;
                    case ParserTreeConstants.SET_DIRECTIVE:
                        expr = Set(child);
                        break;
                    case ParserTreeConstants.DIRECTIVE:
                        expr = new Directive(child);
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
                    return Expression.Constant(int.Parse(node.Literal, CultureInfo.InvariantCulture));;

                case ParserTreeConstants.STRING_LITERAL:
                    return new VelocityString(node);
                case ParserTreeConstants.AND_NODE:
                    return And(node);
                    return new BinaryMathematicalExpression(node, MathematicalOperation.And);
                case ParserTreeConstants.OR_NODE:
                    return Or(node);
                    return new BinaryMathematicalExpression(node, MathematicalOperation.Or);
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
                    return new BinaryMathematicalExpression(node, MathematicalOperation.Add);
                case ParserTreeConstants.SUBTRACT_NODE:
                    return new BinaryMathematicalExpression(node, MathematicalOperation.Subtract);
                case ParserTreeConstants.MUL_NODE:
                    return new BinaryMathematicalExpression(node, MathematicalOperation.Multiply);
                case ParserTreeConstants.DIV_NODE:
                    return new BinaryMathematicalExpression(node, MathematicalOperation.Divide);
                case ParserTreeConstants.MOD_NODE:
                    return new BinaryMathematicalExpression(node, MathematicalOperation.Modulo);
                //Code
                case ParserTreeConstants.ASSIGNMENT:
                    return new AssignmentExpression(node);
                case ParserTreeConstants.REFERENCE:
                    return new DynamicReference(node);
                case ParserTreeConstants.OBJECT_ARRAY:
                    return Array(node);
                case ParserTreeConstants.INTEGER_RANGE:
                    return new IntegerRangeExpression(node);
                case ParserTreeConstants.EXPRESSION:
                    return Expr(node);
                default:
                    throw new NotSupportedException("Node type not supported in an expression: " + node.GetType().Name);
            }
        }


        private static Expression Set(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTSetDirective))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException("node", "Expected only one child");

            return Expr(node.GetChild(0));
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

        private static Expression Array(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTObjectArray))
                throw new ArgumentOutOfRangeException("node");

            var elements = node.GetChildren()
                .Select(Operand);

            return Expression.New(MethodHelpers.ListConstructorInfo, Expression.NewArrayInit(typeof(object), elements));
        }

        private static void GetBinaryExpressionOperands(INode node, out Expression left, out Expression right)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.ChildrenCount != 2)
                throw new NotImplementedException("Expected exactly two children for a binary expression");

            left = Operand(node.GetChild(0));
            right = Operand(node.GetChild(1));
        }

        private static Expression BinaryLogicalExpression(Func<Expression, Expression, MethodInfo, Expression> generator, INode node, MethodInfo implementation = null)
        {
            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            left = VelocityExpressions.CoerceToBoolean(left);
            right = VelocityExpressions.CoerceToBoolean(right);

            return generator(left, right, implementation);
        }

        private static Expression BinaryComparisonExpression(Func<Expression, Expression, bool, MethodInfo, Expression> generator, INode node, MethodInfo implementation)
        {
            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            left = VelocityExpressions.ConvertIfNeeded(left, typeof(object));
            right = VelocityExpressions.ConvertIfNeeded(right, typeof(object));

            return  generator(left, right, false, implementation);
        }


        #region Logical Comparators

        /// <summary>
        /// Builds an equals expression from an ASTLTNode
        /// </summary>
        /// <param name="node">The ASTLTNode to build the less than Expression from</param>
        /// <returns>An expression representing the less than operation</returns>
        private static Expression LessThan(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTLTNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryComparisonExpression(Expression.LessThan, node, MethodHelpers.LessThanMethodInfo);
        }


        private static Expression LessThanOrEqual(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTLENode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryComparisonExpression(Expression.LessThanOrEqual, node, MethodHelpers.LessThanOrEqualMethodInfo);
        }

        /// <summary>
        /// Builds a greater than expression from an ASTGTNode
        /// </summary>
        /// <param name="node">The ASTGTNode to build the greater than Expression from</param>
        /// <returns>An expression representing the greater than operation</returns>
        private static Expression GreaterThan(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTGTNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryComparisonExpression(Expression.GreaterThan, node, MethodHelpers.GreaterThanMethodInfo);
        }
        /// <summary>
        /// Builds a greater than or equal to expression from an ASTGENode
        /// </summary>
        /// <param name="node">The ASTGENode to build the equals Expression from</param>
        /// <returns>An expression representing the greater than or equal to operation</returns>
        private static Expression GreaterThanOrEqual(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTGENode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryComparisonExpression(Expression.GreaterThanOrEqual, node, MethodHelpers.GreaterThanOrEqualMethodInfo);
        }

        /// <summary>
        /// Builds an equals expression from an ASTEQNode
        /// </summary>
        /// <param name="node">The ASTEQNode to build the equals Expression from</param>
        /// <returns>An expression representing the equals operation</returns>
        private static Expression Equal(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTEQNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryComparisonExpression(Expression.Equal, node, MethodHelpers.EqualMethodInfo);
        }

        /// <summary>
        /// Builds a not equal expression from an ASTNENode
        /// </summary>
        /// <param name="node">The ASTNENode to build the not equal Expression from</param>
        /// <returns>An expression representing the not equal  operation</returns>
        private static Expression NotEqual(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTNENode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryComparisonExpression(Expression.NotEqual, node, MethodHelpers.NotEqualMethodInfo);
        }

        #endregion

        #region Logical

        private static Expression Not(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTNotNode))
                throw new ArgumentOutOfRangeException("node");

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException("node");

            var operand = Operand(node.GetChild(0));
            var expression = VelocityExpressions.CoerceToBoolean(operand);

            return Expression.Not(expression);
        }

        private static Expression And(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTAndNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogicalExpression(Expression.AndAlso, node);
        }

        private static Expression Or(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTOrNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogicalExpression(Expression.OrElse, node);
        }


        #endregion
    }
}
