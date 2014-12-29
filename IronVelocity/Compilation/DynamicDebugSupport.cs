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
        private static readonly MethodInfo _setMemberCallSite = typeof(CallSiteHelpers).GetMethod("SetMemberCallSite", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _getMemberCallSite = typeof(CallSiteHelpers).GetMethod("GetMemberCallSite", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _invokeMemberCallSite = typeof(CallSiteHelpers).GetMethod("InvokeMemberCallSite", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _logicalOperationCallSite = typeof(CallSiteHelpers).GetMethod("LogicalOperationCallSite", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _mathematicalOperationCallSite = typeof(CallSiteHelpers).GetMethod("MathematicalOperationCallSite", BindingFlags.Static | BindingFlags.Public);

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
            return base.VisitExtension(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var delegateType = node.DelegateType;
            var siteType = _callSiteType.MakeGenericType(delegateType);

            var callSiteField = Expression.Field(null,
                _builder.DefineField("callsite$" + callSiteId++, siteType, FieldAttributes.Static | FieldAttributes.PrivateScope | FieldAttributes.Private)
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
                Expression.Assign(callSiteField, InitalizeCallSite(node))
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

        private MethodCallExpression InitalizeCallSite(DynamicExpression node)
        {
            MethodInfo callSiteInitMethod = null;
            object[] args = null;
            if (node.Binder is GetMemberBinder)
            {
                callSiteInitMethod = _getMemberCallSite;
                args = new[] { ((GetMemberBinder)node.Binder).Name };
            }
            else if (node.Binder is SetMemberBinder)
            {
                callSiteInitMethod = _setMemberCallSite;
                args = new[] { ((SetMemberBinder)node.Binder).Name };
            }
            else if (node.Binder is InvokeMemberBinder)
            {
                var invokeBinder = node.Binder as InvokeMemberBinder;
                callSiteInitMethod = _invokeMemberCallSite;
                args = new object[] { invokeBinder.Name, invokeBinder.CallInfo.ArgumentCount };
            }
            else if (node.Binder is VelocityBinaryLogicalOperationBinder)
            {
                callSiteInitMethod = _logicalOperationCallSite;
                args = new object[] { ((VelocityBinaryLogicalOperationBinder)node.Binder).Operation };
            }
            else if (node.Binder is VelocityBinaryMathematicalOperationBinder)
            {
                callSiteInitMethod = _mathematicalOperationCallSite;
                args = new object[] { ((VelocityBinaryMathematicalOperationBinder)node.Binder).Operation };
            }

            if (callSiteInitMethod == null)
                throw new NotImplementedException();

            var fullMethod = callSiteInitMethod.MakeGenericMethod(node.DelegateType);
            return Expression.Call(fullMethod, args.Select(Expression.Constant).ToArray());
        }



        public static class CallSiteHelpers
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static CallSite<T> GetMemberCallSite<T>(string memberName)
                where T : class
            {
                return CallSite<T>.Create(BinderHelper.Instance.GetGetMemberBinder(memberName));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static CallSite<T> SetMemberCallSite<T>(string memberName)
                where T : class
            {
                return CallSite<T>.Create(BinderHelper.Instance.GetSetMemberBinder(memberName));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static CallSite<T> InvokeMemberCallSite<T>(string name, int argumentCount)
                where T : class
            {
                return CallSite<T>.Create(BinderHelper.Instance.GetInvokeMemberBinder(name, argumentCount));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static CallSite<T> LogicalOperationCallSite<T>(LogicalOperation op)
                where T : class
            {
                return CallSite<T>.Create(BinderHelper.Instance.GetBinaryLogicalOperationBinder(op));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static CallSite<T> MathematicalOperationCallSite<T>(ExpressionType type)
                where T : class
            {
                return CallSite<T>.Create(BinderHelper.Instance.GetBinaryMathematicalOperationBinder(type));
            }
        }


    }
}
