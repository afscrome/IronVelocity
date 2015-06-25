using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class ReferenceExpression : VelocityExpression
    {
        public ASTReferenceMetadata Metadata { get; private set; }
        public Expression Value { get; private set; }

        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.Reference; } }
        public override Type Type
        {
            get
            {

                return Metadata.RefType == ASTReferenceMetadata.ReferenceType.Runt
                    ? typeof(string)
                    : Value.Type;
            }
        }

        public ReferenceExpression(ASTReferenceMetadata metadata, Expression value)
        {
            Metadata = metadata;
            Value = value;
        }

        public override Expression Reduce()
        {
            if (Metadata.RefType == ASTReferenceMetadata.ReferenceType.Runt)
                return Expression.Constant(Metadata.RootString);

            return Value;
        }

    }

}
