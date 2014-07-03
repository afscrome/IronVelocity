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


        protected override Expression VisitExtension(Expression node)
        {
            var renderableReference = node as RenderableVelocityReference;
            if (renderableReference != null)
                return VisitRenderableReference(renderableReference);

            return base.VisitExtension(node);
        }


        protected virtual Expression VisitRenderableReference(RenderableVelocityReference node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var reference = node.Reference;
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
                        return base.VisitExtension(node);
                    }
                    continue;
                }

                var invoke = child as MethodInvocationExpression;
                if (invoke != null)
                {
                    //TODO: If we can't staticly type part, return a partial static typed expression rather than 100% dynamic
                    if (invoke.Arguments.Any(x => !IsConstantType(x)))
                        return base.VisitExtension(node);

                    var method = ReflectionHelper.ResolveMethod(soFar.Type, invoke.Name, GetArgumentTypes(invoke.Arguments));


                    //TODO: If we can't resolve a method, should probably short circuit and return null right away
                    if (method == null)
                    {
                        //TODO: p - no reason not to staticly type, just needs more work to be supported
                        return base.VisitExtension(node);
                    }
                    else if (method.ReturnType == typeof(void))
                    {
                        //TODO: fix - no reason not to staticly type, just needs more work to be supported
                        return base.VisitExtension(node);
                    }
                    else
                    {
                        soFar = ReflectionHelper.ConvertMethodParamaters(method, soFar, invoke.Arguments.Select(x => new DynamicMetaObject(x, BindingRestrictions.Empty)).ToArray());
                    }
                }
                else
                {
                    throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);
                }

            }

            return base.Visit(new RenderableExpression(soFar, reference.Metadata));
        }

        private Type[] GetArgumentTypes (IReadOnlyList<Expression> expressions)
        {
            var types = new Type[expressions.Count];

            for (int i = 0; i < expressions.Count; i++)
            {
                var constant = expressions[i] as ConstantExpression;
                types[i] = constant == null
                    ? null
                    : expressions[i].Type;
            }
            return types;
        }

        private bool IsConstantType(Expression expression)
        {
            if (expression is ConstantExpression)
                return true;

            //Interpolated & dictionary strings will always return the same type
            if (expression is StringExpression || expression is InterpolatedStringExpression || expression is DictionaryStringExpression)
                return true;

            //Value types can't be inherited from, nor can they be null
            if (expression.Type.IsValueType)
                return true;

            return false;
        }

    }
}
