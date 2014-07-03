using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;

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

            //TODO: can we staticly type more than just global variables?
            if (!_globalTypeMap.TryGetValue(variable.Name, out staticType))
                return base.VisitExtension(node);

            //return base.VisitExtension(node);

            if (reference.Additional.OfType<MethodInvocationExpression>().Any())
                return base.VisitExtension(node);

            bool isStatic = true;
            Expression soFar = Expression.Convert(variable.Reduce(), staticType);

            foreach (var child in reference.Additional)
            {
                var getMember = child as PropertyAccessExpression;
                if (getMember != null)
                {
                    soFar = Expression.Property(soFar, getMember.Name);
                    continue;
                }

                var invoke = child as MethodInvocationExpression;
                if (invoke != null)
                {
                    throw new NotImplementedException();
                    //soFar = Expression.Call(soFar, invoke.Name, invoke.Arguments.ToArray());
                }
                throw new NotSupportedException("Node type not supported in a Reference: " + child.GetType().Name);

            }

            return base.Visit(soFar);

            /*
            node.Reference.
            var reduced = base.VisitExtension(node);
            return Expression.Convert(reduced, staticType);

            //var inner = VisitExtension(node.Expression);
            //return new RenderableExpression(inner, node.MetaData).Reduce();
            */
        }

    }
}
