using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class DynamicReference : VelocityExpression
    {
        public ASTReferenceMetadata MetaData { get; private set; }
        private readonly INode _node;
        public Expression Value { get; private set; }

        public DynamicReference(INode node)
            : base(node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var refNode = node as ASTReference;
            if (refNode == null)
                throw new ArgumentOutOfRangeException("node");

            MetaData = new ASTReferenceMetadata(refNode);
            _node = node;

            if (MetaData.RefType == ASTReferenceMetadata.ReferenceType.Runt)
                Value = Expression.Constant(MetaData.RootString);
            else
            {
                Value = new VariableReference(MetaData.RootString);

                for (int i = 0; i < _node.ChildrenCount; i++)
                {
                    var child = _node.GetChild(i);
                    if (child is ASTIdentifier)
                        Value = new DynamicGetMemberExpression(child, Value); //new DynamicGetMemberExpression(child.FirstToken.Image, value);
                    else if (child is ASTMethod)
                        Value = new DynamicInvokeExpression(child, Value);
                    else
                        throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);
                }
            }
        }

        public override Expression Reduce()
        {
            return Value;
        }
    }

    public class RenderableDynamicReference : VelocityExpression
    {
        public DynamicReference Reference { get; private set; }

        public RenderableDynamicReference(DynamicReference reference)
        {
                Reference = reference;
        }

        public override Expression Reduce()
        {
            if (Reference.MetaData.Escaped)
            {
                return Expression.Condition(
                    Expression.NotEqual(Reference, Expression.Constant(null, Reference.Type)),
                    Expression.Constant(Reference.MetaData.EscapePrefix + Reference.MetaData.NullString),
                    Expression.Constant(Reference.MetaData.EscapePrefix + "\\" + Reference.MetaData.NullString)
                );
            }
            else
            {
                var prefix = Reference.MetaData.EscapePrefix + Reference.MetaData.MoreString;
                var NullValue = Expression.Constant(Reference.MetaData.EscapePrefix + prefix + Reference.MetaData.NullString);

                //If the literal has not been escaped (has an empty prefix), then we can return a simple Coalesce expression
                if (String.IsNullOrEmpty(prefix))
                    return Expression.Coalesce(Reference, NullValue);

                //Otherwise we have to do a slightly more complicated result
                var _evaulatedResult = Expression.Parameter(typeof(object), "tempEvaulatedResult");
                return Expression.Block(
                    new[] { _evaulatedResult },
                    Expression.Assign(_evaulatedResult, Reference),
                    Expression.Condition(
                        Expression.NotEqual(_evaulatedResult, Expression.Constant(null, _evaulatedResult.Type)),
                        Expression.Call(
                            MethodHelpers.StringConcatMethodInfo,
                            Expression.Convert(Expression.Constant(prefix), typeof(object)),
                            VelocityExpressions.ConvertIfNeeded(_evaulatedResult, typeof(object))
                        ),
                        NullValue
                    )
                );
            }
        }

        //public override Type Type { get { return typeof(string); } }
    }
}
