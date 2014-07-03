using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using System;
using System.Dynamic;
using System.Linq;
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
            if (!String.IsNullOrEmpty(fileName)) { 
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
            return base.VisitExtension(node);
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
        private static readonly ConstructorInfo _setMemberBinderConstructor = typeof(VelocitySetMemberBinder).GetConstructor(new[] { typeof(string) });
        private static readonly ConstructorInfo _invokeMemberBinderConstructor = typeof(VelocityInvokeMemberBinder).GetConstructor(new[] { typeof(string), typeof(CallInfo) });
        private static readonly ConstructorInfo _binaryMathematicalOperationBinderConstructor = typeof(VelocityBinaryMathematicalOperationBinder).GetConstructor(new[] { typeof(ExpressionType) });
        private static readonly ConstructorInfo _binaryLogicalOperationBinderConstructor = typeof(VelocityBinaryLogicalOperationBinder).GetConstructor(new[] { typeof(LogicalOperation) });

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
                var invokeBinder = binder as VelocityInvokeMemberBinder;
                if (invokeBinder != null)
                {
                    var name = invokeBinder.Name;

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
                else
                {
                    var setBinder = binder as VelocitySetMemberBinder;
                    if (setBinder != null)
                    {
                        var name = setBinder.Name;
                        return Expression.New(
                            _setMemberBinderConstructor,
                            Expression.Constant(name)
                        );
                    }
                    else {
                        var logical = binder as VelocityBinaryLogicalOperationBinder;
                        if (logical != null)
                        {
                            var type = Expression.Constant(logical.Operation);
                            return Expression.New(_binaryLogicalOperationBinderConstructor, type);
                        }
                        else
                        {
                            var mathematical = binder as VelocityBinaryMathematicalOperationBinder;
                            if (mathematical != null)
                            {
                                var type = Expression.Constant(mathematical.Operation);
                                return Expression.New(_binaryMathematicalOperationBinderConstructor, type);
                            }
                        }
                    }

                }
            }

            throw new ArgumentOutOfRangeException("binder");
        }
    }
}
