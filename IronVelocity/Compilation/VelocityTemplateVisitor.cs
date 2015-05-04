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
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation
{
    public class VelocityTemplateVisitor : VelocityExpressionVisitor
    {
        private readonly bool _includeLineNumbers = true;

        public VelocityTemplateVisitor(IDictionary<string, object> globals, TypeBuilder typeBuilder, string fileName)
        {
            _availableGlobals = globals == null
                ? new Dictionary<string, object>()
                : new Dictionary<string,object>(globals);

            _usedGlobals = new Dictionary<string, ConstantExpression>();



            _builder = typeBuilder;
            if (!String.IsNullOrEmpty(fileName))
            {
                _symbolDocument = Expression.SymbolDocument(fileName);
            }
        }


        protected override Expression VisitDynamic(DynamicExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var args = VisitArguments(node.Arguments);

            bool signatureChanged = false;
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i] != node.Arguments[i])
                {
                    signatureChanged = true;
                    break;
                }
            }

            node = signatureChanged
                ? Expression.Dynamic(node.Binder, node.Type, args)
                : node.Update(args);

            if (_includeLineNumbers)
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


                var body = Expression.Call(
                        Expression.Field(
                            Expression.Assign(site, siteConstant),
                            siteConstant.Type.GetField("Target")
                        ),
                        node.DelegateType.GetMethod("Invoke"),
                        arguments
                    );

                return VisitTemporaryVariableScope(new TemporaryVariableScopeExpression(
                    site,
                    Visit(body)
                ));
            }

            return node;
        }

        #region Static Globals
        private readonly IDictionary<string, object> _availableGlobals;
        private readonly IDictionary<string, ConstantExpression> _usedGlobals;







        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            return node.Update(VisitArguments(node.Expressions));
        }


        protected override Expression VisitVariable(VariableExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            ConstantExpression constant = null;
            if (!_usedGlobals.TryGetValue(node.Name, out constant))
            {
                object value;
                if (_availableGlobals.TryGetValue(node.Name, out value) )
                {
                    if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(value.GetType()))
                    {
                        _availableGlobals.Remove(node.Name);
                    }
                    _availableGlobals[node.Name] = constant = Expression.Constant(value);
                }
            }

            return constant == null
                ? base.VisitVariable(node)
                : Visit(constant);
        }

        protected override Expression VisitPropertyAccess(PropertyAccessExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var target = Visit(node.Target);

            if (IsConstantType(target))
            {
                return ReflectionHelper.MemberExpression(node.Name, target.Type, target)
                    ?? Visit(Constants.NullExpression);
            }

            return base.VisitPropertyAccess(node.Update(target));
        }

        protected override Expression VisitMethodInvocation(MethodInvocationExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var newTarget = Visit(node.Target);
            var args = VisitArguments(node.Arguments);

            // If the target has a static type, and so do all arguments, we can try to staticly type the method call
            if (IsConstantType(newTarget) && args.All(IsConstantType))
            {
                var method = ReflectionHelper.ResolveMethod(newTarget.Type, node.Name, GetArgumentTypes(args));

                return method == null
                    ? Constants.NullExpression
                    : ReflectionHelper.ConvertMethodParameters(method, newTarget, args.Select(x => new DynamicMetaObject(x, BindingRestrictions.Empty)).ToArray());
            }

            return base.VisitMethodInvocation(node.Update(newTarget, args));
        }

        private static Type[] GetArgumentTypes(IReadOnlyList<Expression> expressions)
        {
            var types = new Type[expressions.Count];

            for (int i = 0; i < expressions.Count; i++)
            {
                types[i] = expressions[i].Type;
            }

            return types;
        }


        protected override Expression VisitDictionary(DictionaryExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            bool changed = true;

            var visitedValues = new Dictionary<string, Expression>(node.Values.Count);
            foreach (var pair in node.Values)
            {
                var value = Visit(pair.Value);
                if (value != pair.Value)
                {
                    changed = true;
                }
                visitedValues[pair.Key] = value;
            }

            var result = changed
                ? new DictionaryExpression(visitedValues)
                : node;

            return result;
        }


        protected override Expression VisitSetDirective(SetDirective node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var left = Visit(node.Left);
            if (left is GlobalVariableExpression)
                throw new InvalidOperationException("Cannot assign to a global variable");

            var right = Visit(node.Right);

            if (!left.Type.IsAssignableFrom(right.Type))
            {
                //If we can't assign from right to left, but can from left to right
                // Then we 
                if (right.Type.IsAssignableFrom(left.Type))
                {
                    var temp = Expression.Parameter(left.Type, "castTemp");

                    return Visit(new TemporaryVariableScopeExpression(temp,
                            Expression.Block(
                                Expression.Assign(temp, Expression.TypeAs(right, left.Type)),
                                Expression.Condition(
                                    Expression.NotEqual(temp, Expression.Constant(null, left.Type)),
                                    node.Update(left, temp),
                                    Constants.VoidReturnValue,
                                    typeof(void)
                                    )
                                )
                            )
                        );
                }
                else
                {
                    //TODO: should we throw an exception if it's impossible to assign?
                    return Constants.VoidReturnValue;
                }
            }

            return base.VisitSetDirective((SetDirective)node.Update(left, right));
        }

        protected override Expression VisitCoerceToBoolean(CoerceToBooleanExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var value = Visit(node.Value);

            if (value.Type == typeof(bool) || value.Type == typeof(bool?))
                return value;


            return node.Update(value);
        }

        protected override Expression VisitRenderableReference(RenderableVelocityReference node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var value = Visit(node.Expression);

            return node.Update(value);
        }

        private IReadOnlyList<Expression> VisitArguments(IReadOnlyList<Expression> arguments)
        {
            bool changed = false;
            var visitedValues = new Expression[arguments.Count];

            int i = 0;
            foreach (var oldValue in arguments)
            {
                var value = Visit(oldValue);
                if (value != oldValue)
                {
                    changed = true;
                    if (value.Type.IsValueType)
                        value = VelocityExpressions.ConvertIfNeeded(value, typeof(object));
                }
                visitedValues[i++] = value;
            }

            return changed
                ? visitedValues
                : arguments;
        }

        /// <summary>
        /// Determines whether an expression will always return the same exact type.  
        /// </summary>
        private static bool IsConstantType(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (expression is DynamicExpression || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(expression.Type))
                return false;

            if (expression is ConstantExpression || expression is GlobalVariableExpression)
                return true;

            //Interpolated & dictionary strings will always return the same type
            if (expression is InterpolatedStringExpression || expression is DictionaryStringExpression
                || expression is DictionaryExpression || expression is ObjectArrayExpression)
                return true;

            if (expression.Type == typeof(void))
                return true;

            //If the return type is sealed, we can't get any subclasses back
            if (expression.Type.IsSealed)
                return true;

            return false;
        }

#endregion

        #region Exception Line Numbers

        private readonly TypeBuilder _builder;
        private readonly Dictionary<object, FieldBuilder> _fieldBuilders = new Dictionary<object, FieldBuilder>();

        private readonly SymbolDocumentInfo _symbolDocument;
        private SourceInfo _currentSourceInfo;
        private int callSiteId = 0;


        public void InitaliseConstants(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .ToDictionary(x => x.Name);

            foreach (var pair in _fieldBuilders)
            {
                fields[pair.Value.Name].SetValue(null, pair.Key);
            }  
        }

        protected override Expression VisitVelocityExpression(VelocityExpression node)
        {
            if (_symbolDocument != null)
            {
                var extension = node as VelocityExpression;
                if (extension != null && extension.SourceInfo != null && extension.SourceInfo != _currentSourceInfo)
                {
                    _currentSourceInfo = extension.SourceInfo;
                    return Expression.Block(
                        Expression.DebugInfo(_symbolDocument, _currentSourceInfo.StartLine, _currentSourceInfo.StartColumn, _currentSourceInfo.EndLine, _currentSourceInfo.EndColumn),
                        Visit(node)
                    );

                }
            }
            return base.VisitVelocityExpression(node);
        }

        /// <summary>
        /// Takes any complex Constant expressions that can't be compiled to IL constants and makes them a static field.
        /// These are then initialised by calling <see cref="InitaliseConstants"/> once the Type has been compiled
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

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



        private static bool CanEmitAsConstant(object value)
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
        #endregion


#region Local Reuse
        private readonly IDictionary<Type, Queue<ParameterExpression>> _availableTemps = new Dictionary<Type, Queue<ParameterExpression>>();
        private readonly IDictionary<ParameterExpression, ParameterExpression> _replacements = new Dictionary<ParameterExpression, ParameterExpression>();

        private int index = 0;
        public IReadOnlyCollection<ParameterExpression> TemporaryVariables
        {
            get { return _availableTemps.SelectMany(x => x.Value).ToList(); }
        }

        protected override Expression VisitTemporaryVariableScope(TemporaryVariableScopeExpression node)
        {
            ParameterExpression previousTemp;
            if (!_replacements.TryGetValue(node.Variable, out previousTemp))
            {
                previousTemp = null;
            }

            var newTemp = _replacements[node.Variable] = GetTemp(node.Variable);

            var result = Visit(node.Body);

            _replacements[node.Variable] = previousTemp;
            _availableTemps[node.Variable.Type].Enqueue(newTemp);

            return result;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            ParameterExpression replacement;
            if (_replacements.TryGetValue(node, out replacement) && replacement != null)
            {
                return base.VisitParameter(replacement);
            }


            return base.VisitParameter(node);
        }

        private ParameterExpression GetTemp(ParameterExpression variable)
        {
            Queue<ParameterExpression> tempQueue;
            if (!_availableTemps.TryGetValue(variable.Type, out tempQueue))
            {
                _availableTemps[variable.Type] = tempQueue = new Queue<ParameterExpression>();
            }

            return tempQueue.Any()
                ? tempQueue.Dequeue()
                : Expression.Parameter(variable.Type, "temp" + index++);

        }
#endregion

    }
}
