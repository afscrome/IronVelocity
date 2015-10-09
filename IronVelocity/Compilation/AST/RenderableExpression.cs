using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class RenderableExpression : VelocityExpression
    {
        public Expression Value { get; }
        public string NullOutput { get; }

        public override Type Type => typeof(void);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.RenderableExpression;

        public RenderableExpression(Expression value, string nullOutput)
        {
            Value = value;
            NullOutput = nullOutput;
        }


        public override Expression Reduce()
        {
            if (Value.Type.IsValueType)
            {
                var type = Value.Type;
                var method = MethodHelpers.OutputValueTypeMethodInfo.MakeGenericMethod(Value.Type);

                return Expression.Call(Constants.OutputParameter, method, Value);
            }
            else
            {
                var method = Value.Type == typeof(string)
                    ? MethodHelpers.OutputStringWithNullFallbackMethodInfo
                    : MethodHelpers.OutputObjectWithNullFallbackMethodInfo ;

                return Expression.Call(Constants.OutputParameter, method, Value, Expression.Constant(NullOutput));
            }
        }

    }
}
