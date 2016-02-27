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

            if (!target.HasValue || args.Any(x => !x.HasValue))
                return Defer(target, args);

            if (target == null)
                return BinderHelper.NullTargetResult(target, errorSuggestion);


            // If an argument has a null value, use a null type so that the resolution algorithm can do implicit null conversions
            var argTypeBuilder = ImmutableArray.CreateBuilder<Type>(args.Length);
            foreach (var arg in args)
            {
                argTypeBuilder.Add(arg.Value == null ? null : arg.LimitType);
            }
                
            OverloadResolutionData<MethodInfo> method;
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

            var restrictions = BinderHelper.CreateCommonRestrictions(target, args);
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

                return BinderHelper.UnresolveableResult(restrictions, errorSuggestion);
            }

            var methodExpression = _methodResolver.ConvertMethodParameters(method, target.Expression, args);
            return new DynamicMetaObject(
                VelocityExpressions.ConvertIfNeeded(methodExpression, ReturnType),
                restrictions
            );
        }

    }
}
