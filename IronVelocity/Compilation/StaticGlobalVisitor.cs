using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using System.Dynamic;

namespace IronVelocity.Compilation
{
    public class StaticGlobalVisitor : ExpressionVisitor
    {

        private readonly IReadOnlyDictionary<string, Type> _globalTypeMap;
        public StaticGlobalVisitor(IReadOnlyDictionary<string, Type> globalTypeMap)
        {            
            _globalTypeMap = globalTypeMap;
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            var args = new Expression[node.Arguments.Count];

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                args[i] = Visit(node.Arguments[i]);
            }

            return Expression.Dynamic(node.Binder, node.Type, args);
        }
        

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            var renderableReference = node as RenderableVelocityReference;
            if (renderableReference != null)
                return VisitRenderableReference(renderableReference);

            /*
            var reference = node as ReferenceExpression;
            if (reference != null)
                return VisitReference(reference);
            */
            var setDirective = node as SetDirective;
            if (setDirective != null)
                return VisitSetDirective(setDirective);

            var coerceToBoolean = node as CoerceToBooleanExpression;
            if (coerceToBoolean != null)
                return VisitCoerceToBooleanExpression(coerceToBoolean);

            var dictionary = node as DictionaryExpression;
            if (dictionary != null)
                return VisitDictionaryExpression(dictionary);

            var variable = node as VariableExpression;
            if (variable != null)
                return VisitVariable(variable);

            var property = node as PropertyAccessExpression;
            if (property != null)
                return VisitPropertyAccess(property);

            var method = node as MethodInvocationExpression;
            if (method != null)
                return VisitMethodInvocationExpression(method);

            return base.VisitExtension(node);
        }

        protected virtual Expression VisitBinaryLogicalExpression(BinaryLogicalExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            if (left.Type == typeof(bool) && right.Type == typeof(bool))
            {
                if (node.Operation == LogicalOperation.And)
                    return Expression.AndAlso(left, right);
                else if (node.Operation == LogicalOperation.Or)
                    return Expression.OrElse(left, right);
            }
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitDictionaryExpression(DictionaryExpression node)
        {
            var args = node.Values.ToDictionary(x => x.Key, x => VelocityExpressions.ConvertIfNeeded(Visit(x.Value), typeof(object)));
            return new DictionaryExpression(args);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            return base.VisitListInit(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var elementType = node.Type.GetElementType();
            var args = node.Expressions.Select(x => VelocityExpressions.ConvertIfNeeded(Visit(x), elementType)).ToList();
            return node.Update(args);
        }

        protected virtual Expression VisitCoerceToBooleanExpression(CoerceToBooleanExpression node)
        {
            var condition = Visit(node.Value);

            return new CoerceToBooleanExpression(condition).Reduce();
        }

        protected virtual Expression VisitSetDirective(SetDirective node)
        {
            if (node.Left is GlobalVariableExpression)
                    throw new InvalidOperationException("Cannot assign to a global variable");

            var left = Visit(node.Left);
            var right = Visit(node.Right);

            if (left.Type != right.Type)
            {
                right = VelocityExpressions.ConvertIfNeeded(right, left.Type);
            }

            return Expression.Assign(left.ReduceExtensions(), right);
        }

        protected virtual Expression VisitRenderableReference(RenderableVelocityReference node)
        {
            var reducedRef = Visit(node.Expression);

            return node.Update(reducedRef);
        }


        protected virtual Expression VisitVariable(VariableExpression node)
        {
            Type staticType;
            if (!_globalTypeMap.TryGetValue(node.Name, out staticType) || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(staticType))
                return base.VisitExtension(node);

            return new GlobalVariableExpression(node, staticType);
        }

        protected virtual Expression VisitPropertyAccess(PropertyAccessExpression node)
        {
            var target = Visit(node.Target);

            return node.Update(target);
        }

        protected virtual Expression VisitMethodInvocationExpression(MethodInvocationExpression node)
        {
            var target = Visit(node.Target);
            var args = new Expression[node.Arguments.Count];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Visit(node.Arguments[i]);
            }

            return node.Update(target, args);
        }





        public static bool IsConstantType(Expression expression)
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(expression.Type))
                return false;

            if (expression is ConstantExpression || expression is GlobalVariableExpression)
                return true;

            //Interpolated & dictionary strings will always return the same type
            if (expression is StringExpression || expression is InterpolatedStringExpression || expression is DictionaryStringExpression
                || expression is DictionaryExpression || expression is ObjectArrayExpression)
                return true;

            //if (expression is MethodCallExpression || expression is PropertyAccessExpression || expression is MemberExpression)
                //return true;

            if (expression.Type == typeof(void))
                return true;


            //If the type is sealed, there
            if (expression.Type.IsSealed)
                return true;

            return false;
        }

    }
}
