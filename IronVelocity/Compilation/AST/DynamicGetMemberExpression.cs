using IronVelocity.Binders;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class DynamicGetMemberExpression : VelocityExpression
    {
        public Expression Target {get; private set;}
        public string Name { get; private set; }
        public DynamicGetMemberExpression(INode node, Expression target)
            : base(node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (target == null)
                throw new ArgumentNullException("target");

            Target = target;
            Name = node.Literal;
        }

        protected override Expression ReduceInternal()
        {
            //TODO: allow for reuse of callsites
            return Expression.Dynamic(
                new VelocityGetMemberBinder(Name),
                typeof(object),
                Target
            );
        }
    }
}
