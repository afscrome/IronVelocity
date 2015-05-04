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
            //TODO: Refactor so that value types don't need to be boxed
            var method = typeof(VelocityOutput).GetMethod("Write", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(object), typeof(string) }, null);

            var value = VelocityExpressions.ConvertIfNeeded(Value, typeof(object));

            return Expression.Call(Constants.OutputParameter, method, value, Expression.Constant(NullOutput));
            /*MethodInfo renderMethod;

            if (Value.Type == typeof(object))
                renderMethod = MethodHelpers.OutputObjectMethodInfo;
            else if(Value.Type == typeof(string))
                renderMethod = MethodHelpers.OutputStringMethodInfo;
            else
                renderMethod = MethodHelpers.OutputObjectMethodInfo;
            */

        }

        public override VelocityExpressionType VelocityExpressionType
        {
            get { return VelocityExpressionType.RenderableExpression; }
        }
    }
}
