using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class DynamicReference : VelocityExpression
    {
        public ASTReferenceMetadata Metadata { get; private set; }
        //public Expression Value { get; private set; }
        public VariableReference BaseVariable { get; private set; }

        public IReadOnlyCollection<Expression> Additional { get; private set; }

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

            var additional = new List<Expression>(node.ChildrenCount);

            Expression soFar = BaseVariable;
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
                if (child is ASTIdentifier)
                    soFar = new DynamicGetMemberExpression(child, soFar);
                else if (child is ASTMethod)
                    soFar = new DynamicInvokeExpression(child, soFar);
                else
                    throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);

                additional.Add(soFar);
            }

            Additional = additional;
        }

        public override Expression Reduce()
        {
            if (Metadata.RefType == ASTReferenceMetadata.ReferenceType.Runt)
                return Expression.Constant(Metadata.RootString);

            return Additional.Any()
                ? Additional.Last()
                : BaseVariable;
        }
    }

}
