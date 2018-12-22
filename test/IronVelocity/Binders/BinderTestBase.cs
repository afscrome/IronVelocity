using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IronVelocity.Tests.Binders
{
    public class BinderTestBase
    {
        protected object InvokeBinder(CallSiteBinder binder, params object[] args)
            => InvokeBinder<object>(binder, args);

        protected T InvokeBinder<T>(CallSiteBinder binder, params object[] args)
        {
            var expression = Expression.Dynamic(binder, typeof(T), args.Select(Expression.Constant));

            var action = Expression.Lambda<Func<T>>(expression)
                .Compile();

            return action();
        }
    }
}
