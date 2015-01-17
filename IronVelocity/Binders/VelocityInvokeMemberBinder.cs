using IronVelocity.Compilation;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Binders
{
    public class VelocityInvokeMemberBinder : InvokeMemberBinder
    {
        public VelocityInvokeMemberBinder(String name, CallInfo callInfo)
            : base(name, true, callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            //Don't support static invocation
            return errorSuggestion;
        }


        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (args == null)
                throw new ArgumentNullException("args");

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
            var argTypeArray = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
			{
                var arg = args[i];
    			 argTypeArray[i] = arg.Value == null
                     ? null
                     : arg.LimitType;
			}

            MethodInfo method;
            Expression result = null;
            bool isAmbigious = false;
            try
            {
                method = ReflectionHelper.ResolveMethod(target.LimitType, Name, argTypeArray);
            }
            catch (AmbiguousMatchException)
            {
                isAmbigious = true;
                method = null;
            }

            if (method == null)
            {
                if(args.Length == 0)
                {
                    result = ReflectionHelper.MemberExpression(Name, target);
                }
                if (result == null)
                {
                    var log = BindingEventSource.Log;
                    if (log.IsEnabled())
                    {
                        var argTypeString = String.Join(",", argTypeArray.Select(x => x.FullName).ToArray());
                        if (isAmbigious)
                            log.InvokeMemberResolutionAmbigious(Name, target.LimitType.FullName, argTypeString);
                        else
                            log.InvokeMemberResolutionFailure(Name, target.LimitType.FullName, argTypeString);
                    }

                    result = Constants.VelocityUnresolvableResult;
                }
            }
            else
            {
                result = ReflectionHelper.ConvertMethodParameters(method, target.Expression, args);

                //Not keen on returning empty string, but this maintains consistency with NVelocity.
                // Otherwise returning void fails with an exception because the DLR can't convert 
                // Returning null causes problems as null indicates the method call failed, and so
                // causes the Identifier to be emitted instead of blank.


                //Dynamic return type is object, but primitives are not objects
                // DLR does not handle boxing to make primitives objects, so do it ourselves
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
