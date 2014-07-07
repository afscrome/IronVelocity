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

            var reference = node as ReferenceExpression;
            if (reference != null)
                return VisitReference(reference);

            var setDirective = node as SetDirective;
            if (setDirective != null)
                return VisitSetDirective(setDirective);

            var coerceToBoolean = node as CoerceToBooleanExpression;
            if (coerceToBoolean != null)
                return VisitCoerceToBooleanExpression(coerceToBoolean);

            var dictionary = node as DictionaryExpression;
            if (dictionary != null)
                return VisitDictionaryExpression(dictionary);

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
            var leftReference = node.Left as ReferenceExpression;
            if(leftReference != null)
            {
                if (!leftReference.Additional.Any() && _globalTypeMap.ContainsKey(leftReference.BaseVariable.Name))
                    throw new InvalidOperationException("Cannot assign to a global variable");
            }

            var left = Visit(node.Left);
            var right = Visit(node.Right);

            if (left.Type != right.Type)
            {
                right = VelocityExpressions.ConvertIfNeeded(right, left.Type);
            }

            return Expression.Assign(left, right);
        }

        protected virtual Expression VisitRenderableReference(RenderableVelocityReference node)
        {
            var reducedRef = Visit(node.Reference);

            return reducedRef == node
                ? base.VisitExtension(node)
                : new RenderableExpression(reducedRef, node.Metadata);
        }


        protected virtual Expression VisitReference(ReferenceExpression node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var reference = node;
            var variable = reference.BaseVariable;
            Type staticType;


            /*
             * TODO: can we staticly type more than just global variables?
             * 
             * e.g. in the following, it would be safe to staticly type $x as an int?
             *      #set($x = 123)
             *      $x.ToString()
             *  
             * problems comes with how we use subviews in zimbra
             *      #set($x = 123)
             *      $core_v2_widget.Render('test.vm')
             *      $x.ToString()
             * 
             * test.vm may set $x to be a non integer value
             */
            if (!_globalTypeMap.TryGetValue(variable.Name, out staticType) || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(staticType))
                return base.VisitExtension(node);

            //Further methods / properties introduce some interesting problems
            //Particularly in the case that the object returned is a sub type, or interface implementation
            // Should be safe to continue if the return type is sealed or a value type.  Maybe other scenarios?
            if (reference.Additional != null && reference.Additional.Count > 1)
                return base.VisitExtension(node);



            Expression soFar = VelocityExpressions.ConvertIfNeeded(variable.Reduce(), staticType);

            foreach (var child in reference.Additional)
            {
                var getMember = child as PropertyAccessExpression;
                if (getMember != null)
                {
                    soFar = ReflectionHelper.MemberExpression(getMember.Name, soFar.Type, soFar);
                    //TODO: Handle scenario property isn't there
                    // Use partial static typing?
                    // Short circuit execution of entire expression?
                    if (soFar == null)
                    {
                        return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(getMember.Type)
                            ? base.VisitExtension(node)
                            : Constants.NullExpression;

                    }
                    continue;
                }

                var invoke = child as MethodInvocationExpression;
                if (invoke != null)
                {
                    var args = invoke.Arguments.Select(x => Visit(x)).ToArray(); ;


                    //TODO: If we can't staticly type part, return a partial static typed expression rather than 100% dynamic
                    if (args.Any(x => !IsConstantType(x)))
                        return base.VisitExtension(node);

                    var method = ReflectionHelper.ResolveMethod(soFar.Type, invoke.Name, GetArgumentTypes(args));

                    //TODO: If we can't resolve a method, should probably short circuit and return null right away
                    if (method == null)
                    {
                        return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(invoke.Type)
                            ? base.VisitExtension(node)
                            : Constants.NullExpression;
                    }
                    else
                    {
                        soFar = ReflectionHelper.ConvertMethodParamaters(method, soFar, args.Select(x => new DynamicMetaObject(x, BindingRestrictions.Empty)).ToArray());
                    }
                }
                else
                {
                    throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);
                }

            }

            return base.Visit(soFar);
        }


        private Type[] GetArgumentTypes (IReadOnlyList<Expression> expressions)
        {
            var types = new Type[expressions.Count];

            for (int i = 0; i < expressions.Count; i++)
            {
                types[i] = expressions[i].Type;
            }

            return types;
        }

        private bool IsConstantType(Expression expression)
        {
            if (expression is ConstantExpression)
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
