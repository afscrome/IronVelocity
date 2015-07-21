using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using IronVelocity.Compilation.Directives;
using NVelocity.Exception;
using NVelocity.Runtime;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation
{
    public class NVelocityNodeToExpressionConverter
    {
        public VelocityExpressionBuilder Builder { get; }
        private readonly RuntimeInstance _runtimeInstance;
        private readonly string _templateName;

        private IDictionary<string, object> _globals;

        public NVelocityNodeToExpressionConverter(VelocityExpressionBuilder builder, RuntimeInstance instance, string templateName, IDictionary<string, object> globals)
        {
            Builder = builder;
            _runtimeInstance = instance;
            _templateName = templateName;
            _globals = globals ?? new Dictionary<string, object>();
        }

        public IReadOnlyCollection<Expression> GetBlockExpressions(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            //ASTprocess is a special case for the root, otherwise it behaves exactly like ASTBlock
            if (!(node is ASTBlock || node is ASTprocess))
                throw new ArgumentOutOfRangeException(nameof(node));

            var expressions = new List<Expression>(node.ChildrenCount);

            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
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
                        expr = Reference(child);
                        break;
                    case ParserTreeConstants.IF_STATEMENT:
                        expr = IfDirective(child);
                        break;
                    case ParserTreeConstants.SET_DIRECTIVE:
                        expr = Set(child);
                        break;
                    case ParserTreeConstants.DIRECTIVE:
                        expr = Directive(child);
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

        public Expression Directive(INode node)
        {
            var directiveNode = (ASTDirective)node;

            if (directiveNode == null)
                throw new ArgumentOutOfRangeException(nameof(node));

            if (directiveNode.DirectiveName == "include")
                throw new NotSupportedException("TODO: #include support");
            if (directiveNode.DirectiveName == "parse")
                throw new NotSupportedException("TODO: #parse support");

            DirectiveExpressionBuilder builder;
            foreach (var customDirective in Builder.CustomDirectives)
            {
                var expr = customDirective.ProcessChildDirective(directiveNode.DirectiveName, directiveNode);
                if (expr != null)
                    return expr;
            }

            if (Builder.DirectiveHandlers.TryGetValue(directiveNode.DirectiveName, out builder))
            {
                return builder.Build(directiveNode, this);
            }
            else
                return new UnrecognisedDirective(directiveNode);
        }

        public ConditionalExpression IfDirective(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!(node is ASTIfStatement))
                throw new ArgumentOutOfRangeException(nameof(node));

            if (node.ChildrenCount < 2)
                throw new ArgumentOutOfRangeException(nameof(node), "Expected at least 2 children");


            var condition = new CoerceToBooleanExpression(Expr(node.GetChild(0)));
            var trueContent = new RenderedBlock(GetBlockExpressions(node.GetChild(1)));
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

                    falseContent = new RenderedBlock(GetBlockExpressions(child.GetChild(0)));
                }
                else if (child is ASTElseIfStatement)
                {
                    var innerCondition = new CoerceToBooleanExpression(Expr(child.GetChild(0)));
                    var innerContent = new RenderedBlock(GetBlockExpressions(child.GetChild(1)));

                    falseContent = Expression.IfThenElse(innerCondition, innerContent, falseContent);
                }
                else

                    throw new InvalidOperationException("Expected: ASTElseStatement, Actual: " + child.GetType().Name);
            }

            return Expression.IfThenElse(condition, trueContent, falseContent);
        }

        public SetDirective Set(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.Type != ParserTreeConstants.SET_DIRECTIVE)
                throw new ArgumentOutOfRangeException(nameof(node));

            return Assignment(node.GetChild(0).GetChild(0));
        }

        public SetDirective Assignment(INode node)
        {
            Expression left, right;
            GetBinaryExpressionOperands<ASTAssignment>(node, out left, out right);

            return new SetDirective(left, right, GetSourceInfoFromINode(node));
        }

        public IntegerRangeExpression IntegerRange(INode node)
        {
            Expression left, right;
            GetBinaryExpressionOperands<ASTIntegerRange>(node, out left, out right);

            return new IntegerRangeExpression(left, right, GetSourceInfoFromINode(node));
        }

        public ObjectArrayExpression ObjectArray(INode node)
        {
            if (!(node is ASTObjectArray))
                throw new ArgumentOutOfRangeException(nameof(node));

            var expressions = new Expression[node.ChildrenCount];
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                expressions[i] = Operand(node.GetChild(i));
            }

            return new ObjectArrayExpression(GetSourceInfoFromINode(node), expressions);
        }


        #region Comparison Expressions

        public ComparisonExpression LessThan(INode node)
        {
            return Comparison<ASTLTNode>(node, ComparisonOperation.LessThan);
        }

        public ComparisonExpression GreaterThan(INode node)
        {
            return Comparison<ASTGTNode>(node, ComparisonOperation.GreaterThan);
        }

        public ComparisonExpression LessThanOrEqual(INode node)
        {
            return Comparison<ASTLENode>(node, ComparisonOperation.LessThanOrEqual);
        }

        public ComparisonExpression GreaterThanOrEqual(INode node)
        {
            return Comparison<ASTGENode>(node, ComparisonOperation.GreaterThanOrEqual);
        }

        public ComparisonExpression Equal(INode node)
        {
            return Comparison<ASTEQNode>(node, ComparisonOperation.Equal);
        }

        public ComparisonExpression NotEqual(INode node)
        {
            return Comparison<ASTNENode>(node, ComparisonOperation.NotEqual);
        }

        private ComparisonExpression Comparison<T>(INode node, ComparisonOperation operation)
            where T : INode
        {
            Expression left, right;
            GetBinaryExpressionOperands<T>(node, out left, out right);

            var lineInfo = GetBinaryExpressionLineInfo(node);
            return new ComparisonExpression(left, right, lineInfo, operation);
        }
        #endregion

        #region Boolean Expressions

        private UnaryExpression Not(INode node)
        {
            if (!(node is ASTNotNode))
                throw new ArgumentOutOfRangeException(nameof(node));

            var operand = Operand(node.GetChild(0));
            var expression = VelocityExpressions.CoerceToBoolean(operand);

            //TODO: Include LineInfo for Not
            return Expression.Not(expression);
        }

        private BinaryExpression And(INode node)
        {
            return BinaryBooleanExpression<ASTAndNode>(Expression.AndAlso, node);
        }

        private BinaryExpression Or(INode node)
        {
            return BinaryBooleanExpression<ASTOrNode>(Expression.OrElse, node);
        }

        private BinaryExpression BinaryBooleanExpression<T>(Func<Expression, Expression, BinaryExpression> generator, INode node)
            where T : INode
        {
            Expression left, right;
            GetBinaryExpressionOperands<T>(node, out left, out right);

            // The expression tree will fail if the types don't *exactly* match the types on the method signature
            // So ensure everything is converted to object
            left = VelocityExpressions.CoerceToBoolean(left);
            right = VelocityExpressions.CoerceToBoolean(right);

            //TODO: Include LineInfo for boolean expressions
            var lineInfo = GetBinaryExpressionLineInfo(node);
            return generator(left, right);
        }

        #endregion

        #region Mathematical Expressions

        public MathematicalExpression Add(INode node)
        {
            return Mathematical<ASTAddNode>(node, MathematicalOperation.Add);
        }

        public MathematicalExpression Subtract(INode node)
        {
            return Mathematical<ASTSubtractNode>(node, MathematicalOperation.Subtract);
        }

        public MathematicalExpression Multiply(INode node)
        {
            return Mathematical<ASTMulNode>(node, MathematicalOperation.Multiply);
        }

        public MathematicalExpression Divide(INode node)
        {
            return Mathematical<ASTDivNode>(node, MathematicalOperation.Divide);
        }


        public MathematicalExpression Modulo(INode node)
        {
            return Mathematical<ASTModNode>(node, MathematicalOperation.Modulo);
        }

        private MathematicalExpression Mathematical<T>(INode node, MathematicalOperation operation)
            where T : INode
        {
            Expression left, right;
            GetBinaryExpressionOperands<T>(node, out left, out right);

            var lineInfo = GetBinaryExpressionLineInfo(node);
            return new MathematicalExpression(left, right, lineInfo, operation);
        }
        #endregion



        public ReferenceExpression Reference(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var refNode = node as ASTReference;
            if (refNode == null)
                throw new ArgumentOutOfRangeException(nameof(node));

            var metadata = new ASTReferenceMetadata(refNode);
            Expression value = Variable(metadata.RootString);

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

        public Expression Variable(string name)
        {
            object value;
            if (_globals.TryGetValue(name, out value))
                return new GlobalVariableExpression(name, value);

            return new VariableExpression(name);
        }

        public Expression Method(INode node, Expression target)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (target == null)
                throw new ArgumentNullException(nameof(target));


            var name = node.FirstToken.Image;
            var arguments = new Expression[node.ChildrenCount - 1];
            //Subsequent arguments are the parameters
            for (int i = 1; i < node.ChildrenCount; i++)
            {
                arguments[i - 1] = (Operand(node.GetChild(i)));
            }

            if (ReflectionHelper.IsConstantType(target) && arguments.All(ReflectionHelper.IsConstantType))
            {
                var method = ReflectionHelper.ResolveMethod(target.Type, name, arguments.Select(x => x.Type).ToArray());
                //TODO: Include debug info
                return method == null
                    ? Constants.NullExpression
                    : ReflectionHelper.ConvertMethodParameters(method, target, arguments.Select(x => new DynamicMetaObject(x, BindingRestrictions.Empty)).ToArray());
            }


            return new MethodInvocationExpression(target, name, arguments, GetSourceInfoFromINode(node));
        }


        public Expression Property(INode node, Expression target)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var name = node.Literal;

            if (ReflectionHelper.IsConstantType(target))
            {
                //TODO: Include debug info
                return ReflectionHelper.MemberExpression(name, target.Type, target)
                    ?? Constants.NullExpression;
            }

            return new PropertyAccessExpression(target, name, GetSourceInfoFromINode(node));
        }

        public Expression NVelocityString(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!(node is ASTStringLiteral))
                throw new ArgumentOutOfRangeException(nameof(node));

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
                    return new DictionaryStringExpression(value, this);
                case VelocityStringType.Interpolated:
                    return InterpolatedString(value);
                default:
                    throw new InvalidOperationException();
            }
        }

        public InterpolatedStringExpression InterpolatedString(string value)
        {
            var parser = _runtimeInstance.CreateNewParser();
            using (var reader = new System.IO.StringReader(value))
            {
                SimpleNode ast;
                try
                {
                    ast = parser.Parse(reader, _templateName);
                }
                catch (ParseErrorException)
                {
                    ast = null;
                }

                //If we fail to parse, the ast returned will be null, so just return our normal string
                if (ast == null)
                    return new InterpolatedStringExpression(Expression.Constant(value));

                //Need to create a nested converter - we need to redirect output 
                //TODO: Don't like this behavior, refactor.  Perhaps remove OutputParameter, and replace via an Expression Visitor?
                var parts = GetBlockExpressions(ast)
                    .ToArray();

                return new InterpolatedStringExpression(parts);
            }
        }

        public VelocityStringType DetermineStringType(string value)
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

        public Expression Expr(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!(node is ASTExpression))
                throw new ArgumentOutOfRangeException(nameof(node));

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException(nameof(node), "Only expected one child");

            var child = node.GetChild(0);
            return Operand(child);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="Unavoidable")]
        public Expression Operand(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

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

        private void GetBinaryExpressionOperands<T>(INode node, out Expression left, out Expression right)
            where T : INode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!(node is T))
                throw new ArgumentOutOfRangeException(nameof(node));

            if (node.ChildrenCount != 2)
                throw new ArgumentOutOfRangeException(nameof(node), "Expected exactly two children for a binary expression");

            left = Operand(node.GetChild(0));
            right = Operand(node.GetChild(1));
        }

        private SourceInfo GetBinaryExpressionLineInfo(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.ChildrenCount != 2)
                throw new ArgumentOutOfRangeException(nameof(node), "Expected exactly two children for a binary expression");

            var startToken = node.GetChild(0).FirstToken;
            var endToken = node.GetChild(1).LastToken;
            return new SourceInfo(startToken.BeginLine, startToken.BeginColumn, endToken.EndLine, endToken.EndColumn);
        }

        private static SourceInfo GetSourceInfoFromINode(INode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return new SourceInfo(
                startLine: node.FirstToken.BeginLine,
                startColumn: node.FirstToken.BeginColumn,
                endLine: node.LastToken.EndLine,
                endColumn: node.LastToken.EndColumn
            );
        }

        public enum VelocityStringType
        {
            Constant,
            Dictionary,
            Interpolated
        }
    }
}
