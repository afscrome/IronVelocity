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
    public class StaticGlobalVisitor : ExpressionVisitor
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

        protected override Expression VisitExtension(Expression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var variable = node as VariableExpression;
            if (variable != null)
                return VisitVariable(variable);

            var property = node as PropertyAccessExpression;
            if (property != null)
                return VisitPropertyAccess(property);

            var method = node as MethodInvocationExpression;
            if (method != null)
                return VisitMethodInvocationExpression(method);

            var dictionary = node as DictionaryExpression;
            if (dictionary != null)
                return VisitDictionaryExpression(dictionary);

            var set = node as SetDirective;
            if (set != null)
                return VisitSetDirective(set);

            var coerceToBool = node as CoerceToBooleanExpression;
            if (coerceToBool != null)
                return VisitCoerceToBooleanExpression(coerceToBool);


            var renderableReference = node as RenderableVelocityReference;
            if (renderableReference != null)
                return VisitRenderableVelocityReference(renderableReference);


            return base.VisitExtension(node);
        }


        protected virtual Expression VisitVariable(VariableExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            Type staticType;
            if (!_globalTypeMap.TryGetValue(node.Name, out staticType) || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(staticType))
                return base.VisitExtension(node);

            return new GlobalVariableExpression(node, staticType);
        }

        protected virtual Expression VisitPropertyAccess(PropertyAccessExpression node)
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

        protected virtual Expression VisitMethodInvocationExpression(MethodInvocationExpression node)
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


        protected virtual Expression VisitDictionaryExpression(DictionaryExpression node)
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


        protected Expression VisitSetDirective(SetDirective node)
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
                    var temp = Expression.Parameter(left.Type);

                    return Expression.Block(
                        new[] { temp },
                        Expression.Assign(temp, Expression.TypeAs(right, left.Type)),
                        Expression.Condition(
                            Expression.NotEqual(temp, Expression.Constant(null, left.Type)),
                            node.Update(left, temp),
                            Constants.VoidReturnValue,
                            typeof(void)
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


        protected Expression VisitCoerceToBooleanExpression(CoerceToBooleanExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var value = Visit(node.Value);

            if (value.Type == typeof(bool) || value.Type == typeof(bool?))
                return value;

            return node.Update(value);
        }

        protected Expression VisitRenderableVelocityReference(RenderableVelocityReference node)
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


#if DEBUG
        //For debugging purposes, it is useful to have - easier to identify where visiting is raising errors

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return base.VisitUnary(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return base.VisitBinary(node);
        }
        protected override Expression VisitBlock(BlockExpression node)
        {
            return base.VisitBlock(node);
        }
        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            return base.VisitCatchBlock(node);
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            return base.VisitConstant(node);
        }
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            return base.VisitDebugInfo(node);
        }
        protected override Expression VisitDefault(DefaultExpression node)
        {
            return base.VisitDefault(node);
        }
        protected override ElementInit VisitElementInit(ElementInit node)
        {
            return base.VisitElementInit(node);
        }
        protected override Expression VisitGoto(GotoExpression node)
        {
            return base.VisitGoto(node);
        }
        protected override Expression VisitIndex(IndexExpression node)
        {
            return base.VisitIndex(node);
        }
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return base.VisitInvocation(node);
        }
        protected override Expression VisitLabel(LabelExpression node)
        {
            return base.VisitLabel(node);
        }
        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            return base.VisitLabelTarget(node);
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda<T>(node);
        }
        protected override Expression VisitListInit(ListInitExpression node)
        {
            return base.VisitListInit(node);
        }
        protected override Expression VisitLoop(LoopExpression node)
        {
            return base.VisitLoop(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return base.VisitMemberAssignment(node);
        }
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return base.VisitMemberInit(node);
        }
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }
        protected override Expression VisitNew(NewExpression node)
        {
            return base.VisitNew(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            return base.VisitRuntimeVariables(node);
        }
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return base.VisitMemberListBinding(node);
        }
        protected override Expression VisitSwitch(SwitchExpression node)
        {
            return base.VisitSwitch(node);
        }
        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            return base.VisitSwitchCase(node);
        }
        protected override Expression VisitTry(TryExpression node)
        {
            return base.VisitTry(node);
        }
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return base.VisitTypeBinary(node);
        }
#endif

    }
}
