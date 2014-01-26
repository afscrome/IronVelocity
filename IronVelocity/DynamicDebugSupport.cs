﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Binders;

namespace IronVelocity
{
    /// <summary>
    /// Visits Dynamic expressions in an expression tree and converts them to an explicit implementation using CallSite
    /// 
    /// This is requried when compiling expression trees to assemblies rather than using dynamic method 
    /// </summary>
    public class DynamicToExplicitCallSiteConvertor : ExpressionVisitor
    {
        private static Type _callSiteTType = typeof(CallSite<>);
        private static ConstructorInfo _callInfoConstructor = typeof(CallInfo).GetConstructor(new[] { typeof(int), typeof(string[]) });

        private readonly TypeBuilder _builder;
        public DynamicToExplicitCallSiteConvertor(TypeBuilder typeBuilder)
        {
            _builder = typeBuilder;
        }


        protected override Expression VisitDynamic(DynamicExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var delegateType = node.DelegateType;
            var siteType = _callSiteTType.MakeGenericType(delegateType);

            var callSiteField = Expression.Field(null,
                _builder.DefineField("callsite$0", siteType, FieldAttributes.Static | FieldAttributes.Public)
            );

            //First argument is the callsite
            var arguments = new[] { callSiteField }
                //Need to recursively visit remaining arguments
                .Concat(node.Arguments.Select(Visit));

            var invoke = Expression.Call(
                     Expression.Field(
                         callSiteField,
                          siteType.GetField("Target")
                     ),
                     delegateType.GetMethod("Invoke"),
                     arguments
                 );


            return Expression.Block(
                //Initalise CallSite if it hasn't already been so
                Expression.IfThen(
                    Expression.Equal(callSiteField, Expression.Constant(null)),
                    Expression.Assign(
                        callSiteField,
                            Expression.Call(
                             siteType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(CallSiteBinder) }, null),
                             CreateBinderExpression(node.Binder, node.Arguments.Count)
                            )
                     )
                ),
                //Now invoke the call site.
                invoke
             );

        }

        private static readonly ConstructorInfo _getMemberBinderConstructor = typeof(VelocityGetMemberBinder).GetConstructor(new[] { typeof(string) });
        private static readonly ConstructorInfo _invokeMemberBinderConstructor = typeof(VelocityInvokeMemberBinder).GetConstructor(new[] { typeof(string), typeof(CallInfo) });

        private static Expression _emptyStringArray = Expression.NewArrayInit(typeof(string));

        private static Expression CreateBinderExpression(CallSiteBinder binder, int argCount)
        {
            var getBinder = binder as VelocityGetMemberBinder;
            if (getBinder != null)
            {
                var name = getBinder.Name;
                return Expression.New(
                    _getMemberBinderConstructor,
                    Expression.Constant(name)
                );
            }
            else
            {
                var setBinder = binder as VelocityInvokeMemberBinder;
                if (setBinder != null)
                {
                    var name = setBinder.Name;

                    return Expression.New(
                        _invokeMemberBinderConstructor,
                        Expression.Constant(name),
                        Expression.New(
                            _callInfoConstructor,
                            Expression.Constant(argCount),
                            _emptyStringArray
                        )
                    );
                }
            }

            throw new ArgumentOutOfRangeException("binder");
        }
    }
}
