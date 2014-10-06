using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    /// <summary>
    /// Reduces a Task<T> to just T when the Task has been completed - this simplifies later binding
    /// & improves performance
    /// </summary>
    public class ReduceTaskOfTExpression : VelocityExpression
    {
        private readonly Expression _inner;

        public ReduceTaskOfTExpression(Expression inner)
        {
            if (!inner.Type.IsGenericType || inner.Type.GetGenericTypeDefinition() != typeof(Task<>))
                throw new ArgumentOutOfRangeException("inner", "Node must be of type Task<T>");

            _inner = inner;
        }

        public override Expression Reduce()
        {
            return Expression.Condition(
                Expression.Property(_inner, "IsCompleted"),
                Expression.Property(_inner, "Result"),
                _inner);
        }
    }
}
