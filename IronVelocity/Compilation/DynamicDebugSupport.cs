using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace IronVelocity.Compilation
{
    /// <summary>
    /// Visits Dynamic expressions in an expression tree and converts them to an explicit implementation using CallSite
    /// 
    /// This is required when compiling expression trees to assemblies rather than using dynamic method 
    /// </summary>
    public class DynamicToExplicitCallSiteConvertor : ExpressionVisitor
    {
        private static Type _callSiteType = typeof(CallSite<>);
        private static ConstructorInfo _callInfoConstructor = typeof(CallInfo).GetConstructor(new[] { typeof(int), typeof(string[]) });

        private readonly TypeBuilder _builder;
        private readonly SymbolDocumentInfo _symbolDocument;
        private SymbolInformation _currentSymbol;
        private int callSiteId = 0;

        public DynamicToExplicitCallSiteConvertor(TypeBuilder typeBuilder, string fileName)
        {
            _builder = typeBuilder;
            if (!String.IsNullOrEmpty(fileName))
            {
                _symbolDocument = Expression.SymbolDocument(fileName);
            }
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (_symbolDocument != null)
            {
                var extension = node as VelocityExpression;
                if (extension != null && extension.Symbols != null && extension.Symbols != _currentSymbol)
                {
                    _currentSymbol = extension.Symbols;
                    return base.VisitBlock(
                        Expression.Block(
                            Expression.DebugInfo(_symbolDocument, _currentSymbol.StartLine, _currentSymbol.StartColumn, _currentSymbol.EndLine, _currentSymbol.EndColumn),
                            node
                        )
                    );

                }
            }
            return base.Visit(node.ReduceExtensions());
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var delegateType = node.DelegateType;
            var siteType = _callSiteType.MakeGenericType(delegateType);

            var callSiteField = Expression.Field(null,
                _builder.DefineField("callsite$" + callSiteId++, siteType, FieldAttributes.Static | FieldAttributes.PrivateScope)
            );

            //First argument is the callsite
            var arguments = new Expression[node.Arguments.Count + 1];
            arguments[0] = callSiteField;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                arguments[i + 1] = Visit(node.Arguments[i]);
            }


            var callSiteInit = Expression.Coalesce(
                callSiteField,
                    Expression.Assign(
                        callSiteField,
                            Expression.Call(
                             siteType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(CallSiteBinder) }, null),
                             CreateBinderExpression(node.Binder, node.Arguments.Count)
                            )
                     )
                );

            return Expression.Call(
                     Expression.Field(
                         callSiteInit,
                          siteType.GetField("Target")
                     ),
                     delegateType.GetMethod("Invoke"),
                     arguments
                 );


        }


        private static readonly PropertyInfo _binderHelperInstanceProperty = typeof(BinderHelper).GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _getMemberBinderMethod = typeof(BinderHelper).GetMethod("GetGetMemberBinder", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);
        private static readonly MethodInfo _setMemberBinderMethod = typeof(BinderHelper).GetMethod("GetSetMemberBinder", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);
        private static readonly MethodInfo _invokeMemberBinderMethod = typeof(BinderHelper).GetMethod("GetInvokeMemberBinder", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string), typeof(int) }, null);
        private static readonly MethodInfo _logicalOperationBinderMethod = typeof(BinderHelper).GetMethod("GetBinaryLogicalOperationBinder", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(LogicalOperation) }, null);
        private static readonly MethodInfo _mathematicalOperationBinderMethod = typeof(BinderHelper).GetMethod("GetBinaryMathematicalOperationBinder", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(ExpressionType) }, null);

        private static Expression _emptyStringArray = Expression.NewArrayInit(typeof(string));

        private static Expression CreateBinderExpression(CallSiteBinder binder, int argCount)
        {
            var getBinder = binder as VelocityGetMemberBinder;
            if (getBinder != null)
            {
                var name = getBinder.Name;
                return Expression.Call(
                    Expression.Property(null, _binderHelperInstanceProperty),
                    _getMemberBinderMethod,
                    Expression.Constant(name)
                    );
            }
            else
            {
                var invokeBinder = binder as VelocityInvokeMemberBinder;
                if (invokeBinder != null)
                {
                    var name = invokeBinder.Name;


                    return Expression.Call(
                        Expression.Property(null, _binderHelperInstanceProperty),
                        _invokeMemberBinderMethod,
                        Expression.Constant(name),
                        Expression.Constant(argCount)
                    );
                }
                else
                {
                    var setBinder = binder as VelocitySetMemberBinder;
                    if (setBinder != null)
                    {
                        var name = setBinder.Name;

                        return Expression.Call(
                            Expression.Property(null, _binderHelperInstanceProperty),
                            _setMemberBinderMethod,
                            Expression.Constant(name)
                            );
                    }
                    else
                    {
                        var logical = binder as VelocityBinaryLogicalOperationBinder;
                        if (logical != null)
                        {
                            return Expression.Call(
                                Expression.Property(null, _binderHelperInstanceProperty),
                                _logicalOperationBinderMethod,
                                Expression.Constant(logical.Operation)
                                );
                        }
                        else
                        {
                            var mathematical = binder as VelocityBinaryMathematicalOperationBinder;
                            if (mathematical != null)
                            {
                                return Expression.Call(
                                    Expression.Property(null, _binderHelperInstanceProperty),
                                    _mathematicalOperationBinderMethod,
                                    Expression.Constant(mathematical.Operation)
                                    );
                            }
                        }
                    }

                }
            }

            throw new ArgumentOutOfRangeException("binder");
        }
    }
}
