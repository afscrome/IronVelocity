using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class ReferenceExpression : VelocityExpression
    {
        public ASTReferenceMetadata Metadata { get; private set; }
        //public Expression Value { get; private set; }
        public VariableExpression BaseVariable { get; private set; }

        public IReadOnlyCollection<Expression> Additional { get; private set; }

        public ReferenceExpression(INode node)
            : base(node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var refNode = node as ASTReference;
            if (refNode == null)
                throw new ArgumentOutOfRangeException("node");

            Metadata = new ASTReferenceMetadata(refNode);

            BaseVariable = new VariableExpression(Metadata.RootString);

            var additional = new List<Expression>(node.ChildrenCount);

            Expression soFar = BaseVariable;
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
                if (child is ASTIdentifier)
                    soFar = new PropertyAccessExpression(child, soFar);
                else if (child is ASTMethod)
                    soFar = new MethodInvocationExpression(child, soFar);
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

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            var baseVariable = visitor.Visit(BaseVariable);
            if (baseVariable == BaseVariable)
                return base.VisitChildren(visitor);


            return visitor.Visit(this.ReduceAndCheck());
        }
    }

}
