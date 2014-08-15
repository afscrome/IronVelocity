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
            if (!(node is ASTAssignment))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

            return new SetDirective(left, right, new SymbolInformation(node));
        }

        public static IntegerRangeExpression IntegerRange(INode node)
        {
            if (!(node is ASTIntegerRange))
                throw new ArgumentOutOfRangeException("node");

            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

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
            if (!(node is ASTLTNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogical(node, LogicalOperation.LessThan);
        }

        public static BinaryLogicalExpression GreaterThan(INode node)
        {
            if (!(node is ASTGTNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogical(node, LogicalOperation.GreaterThan);
        }

        public static BinaryLogicalExpression LessThanOrEqual(INode node)
        {
            if (!(node is ASTLENode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogical(node, LogicalOperation.LessThanOrEqual);
        }

        public static BinaryLogicalExpression GreaterThanOrEqual(INode node)
        {
            if (!(node is ASTGENode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogical(node, LogicalOperation.GreaterThanOrEqual);
        }

        public static BinaryLogicalExpression Equal(INode node)
        {
            if (!(node is ASTEQNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogical(node, LogicalOperation.Equal);
        }

        public static BinaryLogicalExpression NotEqual(INode node)
        {
            if (!(node is ASTNENode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryLogical(node, LogicalOperation.NotEqual);
        }

        private static BinaryLogicalExpression BinaryLogical(INode node, LogicalOperation operation)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.ChildrenCount != 2)
                throw new NotImplementedException("Expected exactly two children for a binary expression");

            var left = Operand(node.GetChild(0));
            var right = Operand(node.GetChild(1));

            return new BinaryLogicalExpression(left,right, new SymbolInformation(node), operation);
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
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTAndNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryBooleanExpression(Expression.AndAlso, node);
        }

        private static Expression Or(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTOrNode))
                throw new ArgumentOutOfRangeException("node");


            return BinaryBooleanExpression(Expression.OrElse, node);
        }

        private static Expression BinaryBooleanExpression(Func<Expression, Expression, Expression> generator, INode node)
        {
            Expression left, right;
            GetBinaryExpressionOperands(node, out left, out right);

            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            left = VelocityExpressions.CoerceToBoolean(left);
            right = VelocityExpressions.CoerceToBoolean(right);

            return generator(left, right);
        }

        private static void GetBinaryExpressionOperands(INode node, out Expression left, out Expression right)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.ChildrenCount != 2)
                throw new ArgumentOutOfRangeException("Expected exactly two children for a binary expression");

            left = Operand(node.GetChild(0));
            right = Operand(node.GetChild(1));
        }

        #endregion

        #region Mathematical Expressions

        public static Expression Add(INode node)
        {
            if (!(node is ASTAddNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryMathematical(node, MathematicalOperation.Add);
        }

        public static Expression Subtract(INode node)
        {
            if (!(node is ASTSubtractNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryMathematical(node, MathematicalOperation.Subtract);
        }

        public static Expression Multiply(INode node)
        {
            if (!(node is ASTMulNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryMathematical(node, MathematicalOperation.Multiply);
        }

        public static Expression Divide(INode node)
        {
            if (!(node is ASTDivNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryMathematical(node, MathematicalOperation.Divide);
        }


        public static Expression Modulo(INode node)
        {
            if (!(node is ASTModNode))
                throw new ArgumentOutOfRangeException("node");

            return BinaryMathematical(node, MathematicalOperation.Modulo);
        }

        private static BinaryMathematicalExpression BinaryMathematical(INode node, MathematicalOperation operation)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.ChildrenCount != 2)
                throw new NotImplementedException("Expected exactly two children for a binary expression");

            var left = Operand(node.GetChild(0));
            var right = Operand(node.GetChild(1));

            return new BinaryMathematicalExpression(left, right, new SymbolInformation(node), operation);
        }
        #endregion

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
                    return new StringExpression(node);

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
                    return new ReferenceExpression(node);
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


    }
}
