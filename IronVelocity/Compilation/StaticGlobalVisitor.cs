using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation
{
    /// <summary>
    ///     Optimises a Velocity Expression tree by replacing dynamic expressions on global variables with static expressions where possible
    /// </summary>
    /// <remarks>
    ///     Whilst replacing known Variables with statically typed variables is simple, the complication comes in that we need to update
    ///     parent expressions so that the statically typed node type is compatible. 
    /// </remarks>
    public class StaticGlobalVisitor : VelocityExpressionVisitor
    {
        private readonly IReadOnlyDictionary<string, Type> _globalTypeMap;

        public StaticGlobalVisitor(IReadOnlyDictionary<string, Type> globalTypeMap)
        {
            _globalTypeMap = globalTypeMap;
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

            return signatureChanged
                ? Expression.Dynamic(node.Binder, node.Type, args)
                : node.Update(args);
        }
       

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

            Type staticType;
            if (!_globalTypeMap.TryGetValue(node.Name, out staticType) || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(staticType))
                return node; // base.VisitExtension(node);

            return new GlobalVariableExpression(node, staticType);
        }

        protected override Expression VisitPropertyAccess(PropertyAccessExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var target = Visit(node.Target);

            if (IsConstantType(target))
            {
                return ReflectionHelper.MemberExpression(node.Name, target.Type, target)
                    ?? Constants.NullExpression;
            }

            return node.Update(target);
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

            return node.Update(newTarget, args);
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

                    return new TemporaryVariableScopeExpression(temp,
                        Expression.Block(
                            Expression.Assign(temp, Expression.TypeAs(right, left.Type)),
                            Expression.Condition(
                                Expression.NotEqual(temp, Expression.Constant(null, left.Type)),
                                node.Update(left, temp),
                                Constants.VoidReturnValue,
                                typeof(void)
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

            return node.Update(left, right);
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




    }
}
