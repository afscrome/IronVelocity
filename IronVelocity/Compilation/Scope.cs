using System;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation
{
    public interface IScope
    {
        Expression GetVariable(string name);
    }

    public class BaseScope : IScope
    {
        private static readonly PropertyInfo _indexerProperty = typeof(VelocityContext).GetProperty("Item", typeof(Expression), new[] { typeof(string) });

        private readonly Expression _context;

        public BaseScope(Expression context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (context.Type != typeof(VelocityContext))
                throw new ArgumentOutOfRangeException("context");

            _context = context;
        }

        public Expression GetVariable(string name)
        {
            return Expression.MakeIndex(_context, _indexerProperty, new[] { Expression.Constant(name) });
        }

    }

}
