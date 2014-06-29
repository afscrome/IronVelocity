using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class DynamicReference : VelocityExpression
    {
        public ASTReferenceMetadata Metadata { get; private set; }
        //public Expression Value { get; private set; }
        public VariableReference BaseVariable { get; private set; }

        private IReadOnlyCollection<INode> Additional { get; set; }

        public DynamicReference(INode node)
            : base(node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var refNode = node as ASTReference;
            if (refNode == null)
                throw new ArgumentOutOfRangeException("node");

            Metadata = new ASTReferenceMetadata(refNode);

            BaseVariable = new VariableReference(Metadata.RootString);

            var additional = new List<INode>(node.ChildrenCount);

            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
                if (child is ASTIdentifier || child is ASTMethod)
                    additional.Add(child);
                else
                    throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);
            }

            Additional = additional;
        }

        public override Expression Reduce()
        {
            if (Metadata.RefType == ASTReferenceMetadata.ReferenceType.Runt)
                return Expression.Constant(Metadata.RootString);

            Expression value = BaseVariable;

            foreach (var child in Additional)
            {
                if (child is ASTIdentifier)
                    value = new DynamicGetMemberExpression(child, value); //new DynamicGetMemberExpression(child.FirstToken.Image, value);
                else if (child is ASTMethod)
                    value = new DynamicInvokeExpression(child, value);
                else
                    throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);
            }

            return value;
        }
    }


    public class DynamicReferenceOriginal : VelocityExpression
    {
        public ASTReferenceMetadata Metadata { get; private set; }
        private readonly INode _node;
        public Expression Value { get; private set; }

        public DynamicReferenceOriginal(INode node)
            : base(node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var refNode = node as ASTReference;
            if (refNode == null)
                throw new ArgumentOutOfRangeException("node");

            Metadata = new ASTReferenceMetadata(refNode);
            _node = node;

            if (Metadata.RefType == ASTReferenceMetadata.ReferenceType.Runt)
                Value = Expression.Constant(Metadata.RootString);
            else
            {
                Value = new VariableReference(Metadata.RootString);

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
}
