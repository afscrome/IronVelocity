using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class AwaitExpression : VelocityExpression
    {
        private readonly Expression _inner;
        private readonly Type _type;

        public override Type Type { get { return _type; } }
        
        public AwaitExpression(Expression inner)
        {
            if (inner.Type == typeof(Task))
            {
                _type = typeof(void);
            }
            else if (inner.Type.IsGenericType && inner.Type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                _type = inner.Type.GetGenericArguments()[0];
            }
            else
            {
                throw new ArgumentOutOfRangeException("inner", "Can only await Task or Task<T> expressions");
            }
        }

        public override Expression Reduce()
        {
            //TODO: Temporary - using Task<T>.Result defeats the point of async as it blocks the current thread when the task has not yet been completed.
            if (_type == typeof(void))
            {
                throw new NotImplementedException();
            }
            else
                return Expression.Property(_inner, "Result");
        }
    }
}
