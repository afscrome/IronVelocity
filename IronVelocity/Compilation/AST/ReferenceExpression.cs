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

        private IReadOnlyCollection<Expression> Additional { get;  set; }

        public ReferenceExpression(ASTReferenceMetadata metadata, VariableExpression baseVariable, IReadOnlyCollection<Expression> additional)
        {
            Metadata = metadata;
            BaseVariable = baseVariable;
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
