using System;
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
    public class DynamicToExplicitCallsiteConvertor : ExpressionVisitor
    {
        private static Type _callSiteTType = typeof(CallSite<>);
        private static ConstructorInfo _callInfoConstructor = typeof(CallInfo).GetConstructor(new[] { typeof(int), typeof(string[]) });

        private readonly TypeBuilder _builder;
        public DynamicToExplicitCallsiteConvertor(TypeBuilder typeBuilder)
        {
            _builder = typeBuilder;
        }


        protected override Expression VisitDynamic(DynamicExpression node)
        {
            var delegateType = node.DelegateType;
            var siteType = _callSiteTType.MakeGenericType(delegateType);

            var callSiteField = Expression.Field(null,
                _builder.DefineField("callsite$0", siteType, FieldAttributes.Static | FieldAttributes.Public)
            );

            //First argument is the callsite
            var arguments = new[] { callSiteField }
                //Need to recusively visit remaining arguments
                .Concat(node.Arguments.Select(Visit));

            var invoke =                 Expression.Call(
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

        private Expression CreateBinderExpression(CallSiteBinder binder, int argCount)
        {
            if (binder is VelocityGetMemberBinder)
            {
                var name = ((VelocityGetMemberBinder)binder).Name;
                return Expression.New(
                    _getMemberBinderConstructor,
                    Expression.Constant(name)
                );
            }
            else if (binder is VelocityInvokeMemberBinder)
            {
                var name = ((VelocityInvokeMemberBinder)binder).Name;

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

            throw new ArgumentOutOfRangeException("binder");
        }
    }
}
