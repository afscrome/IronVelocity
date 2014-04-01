using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Diagnostics;
using IronVelocity.Compilation;
using System.Reflection;
using System.Numerics;

namespace IronVelocity.Binders
{
    public abstract class VelocityBinaryOperationBinder : BinaryOperationBinder
    {
        protected VelocityBinaryOperationBinder(ExpressionType type)
            : base(type)
        {
        }

        public sealed override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            if (!arg.HasValue)
                Defer(arg);


            var restrictions = GetBindingRestriction(target).Merge(GetBindingRestriction(arg));
            //If either of the values are null, the result will be null
            if (target.Value == null || arg.Value == null)
            {
                return new DynamicMetaObject(
                    Expression.Default(ReturnType),
                    restrictions
                );
            }

            //Convert the expression types, either by implicit conversion to a common type, or to the runtime type
            Expression left = ReflectionHelper.CanBeImplicitlyConverted(target.RuntimeType, arg.RuntimeType)
                ? Expression.Convert(target.Expression, arg.RuntimeType)
                : VelocityExpressions.ConvertIfNeeded(target.Expression, target.RuntimeType, target.RuntimeType);

            Expression right = ReflectionHelper.CanBeImplicitlyConverted(arg.RuntimeType, target.RuntimeType)
                ? Expression.Convert(arg.Expression, target.RuntimeType)
                : VelocityExpressions.ConvertIfNeeded(arg.Expression, arg.RuntimeType, arg.RuntimeType);

            var expression = FallbackBinaryOperationExpression(left, right);

            return new DynamicMetaObject(
                VelocityExpressions.ConvertIfNeeded(expression, ReturnType),
                restrictions
            );
        }

        protected abstract Expression FallbackBinaryOperationExpression(Expression left, Expression right);

        protected BindingRestrictions GetBindingRestriction(DynamicMetaObject value)
        {
            if (value.Value == null)
                return BindingRestrictions.GetInstanceRestriction(value.Expression, null);
            else
                return BindingRestrictions.GetTypeRestriction(value.Expression, value.RuntimeType);
        }

    }

}
