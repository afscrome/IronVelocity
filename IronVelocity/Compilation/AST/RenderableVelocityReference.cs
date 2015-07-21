using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class RenderableVelocityReference : VelocityExpression
    {
        public Expression Reference { get; }
        public ASTReferenceMetadata Metadata { get; }

        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.RenderableReference; } }
        public override Type Type { get { return typeof(void); } }

        public RenderableVelocityReference(ReferenceExpression reference)            
        {
            Reference = reference;
            Metadata = reference.Metadata;
        }

        public override Expression Reduce()
        {
            if (Metadata.Escaped)
                return EscapedOutput();

            var prefix = Metadata.EscapePrefix + Metadata.MoreString;

            if (!String.IsNullOrEmpty(prefix))
            {
                if (!String.IsNullOrEmpty(Metadata.EscapePrefix))
                {
                    throw new NotImplementedException("TODO: Prefix with non empty Escape Prefix support");
                }
                else
                {
                    return Expression.Block(
                            Expression.Call(Constants.OutputParameter, MethodHelpers.OutputStringMethodInfo, Expression.Constant(prefix)),
                            new RenderableExpression(Reference, Metadata.NullString)
                        );
                }

            }

            var nullValue = Metadata.EscapePrefix + prefix + Metadata.NullString;

            //TODO: How to handle prefix?

            return new RenderableExpression(Reference, nullValue);
        }

        private Expression EscapedOutput()
        {
            var notNullResult = Expression.Constant(Metadata.EscapePrefix + Metadata.NullString);

            if (Reference.Type.IsValueType)
            {
                return notNullResult;
            }
            else
            {
                var result = Expression.Condition(
                    Expression.NotEqual(Reference, Expression.Constant(null, Reference.Type)),
                    notNullResult,
                    Expression.Constant(Metadata.EscapePrefix + "\\" + Metadata.NullString)
                );

                return Expression.Call(Constants.OutputParameter, MethodHelpers.OutputStringMethodInfo, result);
            }
        }


    }
}
