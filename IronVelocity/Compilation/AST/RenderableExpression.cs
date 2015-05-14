using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class RenderableExpression : VelocityExpression
    {
        public Expression Value { get; private set; }
        public string NullOutput { get; private set; }

        public override Type Type { get { return typeof(void); } }

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

        public override VelocityExpressionType VelocityExpressionType
        {
            get { return VelocityExpressionType.RenderableExpression; }
        }
    }
}
