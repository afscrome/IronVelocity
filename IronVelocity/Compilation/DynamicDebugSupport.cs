using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using System;
using System.Collections.Generic;
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

        private readonly TypeBuilder _builder;
        private readonly Dictionary<object, FieldBuilder> _fieldBuilders = new Dictionary<object, FieldBuilder>();

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


        public void InitaliseConstants(Type type)
        {
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .ToDictionary(x => x.Name);

            foreach (var pair in _fieldBuilders)
            {
                fields[pair.Value.Name].SetValue(null, pair.Key);
            }  
        }

        /// <summary>
        /// Expands any VelocityExpressions with line information to emit DebugInfo expressions to support debugging
        /// </summary>
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
                            Visit(node)
                        )
                    );

                }
            }
            return base.VisitExtension(node);
        }

        /// <summary>
        /// Takes any complex Constant expressions that can't be compiled to IL constants and makes them a static field.
        /// These are then initialised by calling <see cref="InitaliseConstants"/> once the Type has been compiled
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null || CanEmitAsConstant(node.Value))
                return base.VisitConstant(node);

            FieldBuilder field;
            if (!_fieldBuilders.TryGetValue(node.Value, out field))
            {
                field = _builder.DefineField(
                    "$constant" + _fieldBuilders.Count,
                    node.Type,
                    FieldAttributes.Private | FieldAttributes.Static
                );
                _fieldBuilders.Add(node.Value, field);
            }

            return Expression.Field(null, field);
        }

        /// <summary>
        /// Converts a dynamic expression into raw CallSite calls
        /// ($site = siteExpr).Target.Invoke($site, *args)
        /// </summary>
        protected override Expression VisitDynamic(DynamicExpression node)
        {
            // Store the callsite as a constant
            var siteConstant = Expression.Constant(CallSite.Create(node.DelegateType, node.Binder));

            var site = Expression.Variable(siteConstant.Type, "$site_" + callSiteId++);

            var arguments = new Expression[node.Arguments.Count + 1];
            arguments[0] = site;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                arguments[i + 1] = Visit(node.Arguments[i]);
            }

            var result = Expression.Block(
                new[] { site },
                Expression.Call(
                    Expression.Field(
                        Expression.Assign(site, siteConstant),
                        siteConstant.Type.GetField("Target")
                    ),
                    node.DelegateType.GetMethod("Invoke"),
                    arguments
                )
            );

            return Visit(result);
        }

        private bool CanEmitAsConstant(object value)
        {
            if (value == null)
                return true;

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return true;
            }
            return value is Type;
        }


    }
}
