using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Binders
{
    public class VelocityInvokeMemberBinder : InvokeMemberBinder
    {
        private IMethodResolver _methodResolver;

        public VelocityInvokeMemberBinder(string name, CallInfo callInfo, IMethodResolver methodResolver)
            : base(name, true, callInfo)
        {
            _methodResolver = methodResolver;
        }

        //Don't support static invocation
        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
            => errorSuggestion;


        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            //TODO: Support dictionary --> arguments
            //TODO: Support optional params?  BindingFlags.OptionalParamBinding

            //If any of the Dynamic Meta Objects don't yet have a value, defer until they have values.  Failure to do this may result in an infinite loop
            if (!target.HasValue)
                return Defer(target);

            // If the target has a null value, then we won't be able to get any fields or properties, so escape early
            // Failure to escape early like this results in an infinite loop
            if (target.Value == null)
            {
                return new DynamicMetaObject(
                    Constants.NullExpression,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );
            }

            // If an argument has a null value, use a null type so that the resolution algorithm can do implicit null conversions
            var argTypeBuilder = ImmutableArray.CreateBuilder<Type>(args.Length);
            foreach (var arg in args)
            {
                argTypeBuilder.Add(arg.Value == null ? null : arg.LimitType);
            }
                
            MethodInfo method;
            Expression result = null;
            bool isAmbigious = false;
            try
            {
                method = _methodResolver.ResolveMethod(target.LimitType.GetTypeInfo(), Name, argTypeBuilder.ToImmutable());
            }
            catch (AmbiguousMatchException)
            {
                isAmbigious = true;
                method = null;
            }

            if (method == null)
            {
                var log = BindingEventSource.Log;
                if (log.IsEnabled())
                {
                    var argTypeString = string.Join(",", argTypeBuilder.Select(x => x.FullName).ToArray());
                    if (isAmbigious)
                        log.InvokeMemberResolutionAmbiguous(Name, target.LimitType.FullName, argTypeString);
                    else
                        log.InvokeMemberResolutionFailure(Name, target.LimitType.FullName, argTypeString);
                }

                result = errorSuggestion?.Expression ?? Constants.VelocityUnresolvableResult;
            }
            else
            {
                result = _methodResolver.ConvertMethodParameters(method, target.Expression, args);
            }

            var restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);

            foreach (var arg in args)
	        {
                var argRestriction = arg.Value == null
                    ? BindingRestrictions.GetInstanceRestriction(arg.Expression, null)
                    : BindingRestrictions.GetTypeRestriction(arg.Expression, arg.LimitType);

                restrictions = restrictions.Merge(argRestriction);
	        }

            return new DynamicMetaObject(
                VelocityExpressions.ConvertIfNeeded(result, ReturnType),
                restrictions
            );
        }

    }
}
