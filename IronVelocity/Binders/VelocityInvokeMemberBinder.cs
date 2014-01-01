using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
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
            throw new NotImplementedException();
        }


        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            //TODO: Support dictionary --> arguments
            //TODO: Support optional params?  BindingFlags.OptionalParamBinding

            //If any of the Dynamic Meta Objects don't yet have a value, defer until they have values.  Failure to do this may result in an infinite loop
            if (!target.HasValue)
                return Defer(target);

            // If the target has a null value, then we won't be able to get any fields or properties, so escape early
            // Failure to escape early like this do this results in an infinite loop
            if (target.Value == null)
            {
                return new DynamicMetaObject(
                    Constants.NullExpression,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );
            }

            MethodInfo method = null;
            var argTypeArray = args.Select(x => x.LimitType).ToArray();
            try
            {
                method = target.LimitType.GetMethod(Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, argTypeArray, null);
            }
            catch (AmbiguousMatchException)
            {
                try
                {
                    method = target.LimitType.GetMethod(Name, BindingFlags.Public | BindingFlags.Instance, null, argTypeArray, null);
                }
                catch (AmbiguousMatchException)
                {
                    Debug.WriteLine(string.Format("Ambiguous match for method '{0}' on type '{1}'", Name, target.LimitType.AssemblyQualifiedName), "Velocity");

                }
            }

            Expression result;

            if (method == null)
            {
                Debug.WriteLine(string.Format("Unable to resolve method '{0}' on type '{1}'", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                result = Constants.VelocityUnresolvableResult;
            }
            else
            {
                var parameters = method.GetParameters();
                var argExpressions = new Expression[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    argExpressions[i] = VelocityExpressions.ConvertParameterIfNeeded(args[i], parameters[i]);
                }


                result = Expression.Call(
                    VelocityExpressions.ConvertReturnTypeIfNeeded(target, method),
                    method,
                    argExpressions
                );

                //Dynamic return type is object, but primitives are not objects
                // DLR does not handle boxing to make primitives objects, so do it ourselves
                result = VelocityExpressions.BoxIfNeeded(result);
            }

            return new DynamicMetaObject(
                result,
                BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
            );
        }

    }
}
