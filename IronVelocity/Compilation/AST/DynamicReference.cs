using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        protected override Expression ReduceInternal()
        {
            return Value;
        }
    }

    public class RenderableDynamicReference : DynamicReference
    {
        public RenderableDynamicReference(INode node)
            : base(node){

        }

        protected override Expression ReduceInternal()
        {
            var expr = base.ReduceInternal();

            //If however we're rendering, we need to do funky stuff to the output, including rendering prefixes, suffixes etc.
            if (MetaData.Escaped)
            {
                return Expression.Condition(
                    Expression.NotEqual(expr, Expression.Constant(null, expr.Type)),
                    Expression.Constant(MetaData.EscapePrefix + MetaData.NullString),
                    Expression.Constant(MetaData.EscapePrefix + "\\" + MetaData.NullString)
                );
            }
            else
            {
                var _evaulatedResult = Expression.Parameter(typeof(object), "tempEvaulatedResult");
                //TODO: this fails if return type is void
                return Expression.Block(
                    new[] { _evaulatedResult },
                    Expression.Assign(_evaulatedResult, expr),
                    Expression.Condition(
                        Expression.NotEqual(_evaulatedResult, Expression.Constant(null, _evaulatedResult.Type)),
                        Expression.Call(
                            MethodHelpers.StringConcatMethodInfo,
                            Expression.Convert(Expression.Constant(MetaData.EscapePrefix + MetaData.MoreString), typeof(object)),
                            VelocityExpressions.ConvertIfNeeded(_evaulatedResult, typeof(object))
                        ),
                        Expression.Constant(MetaData.EscapePrefix + MetaData.EscapePrefix + MetaData.MoreString + MetaData.NullString)
                    )
                );
            }
        }   
    }
}
