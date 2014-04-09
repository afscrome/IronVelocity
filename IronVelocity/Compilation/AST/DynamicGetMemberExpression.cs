using IronVelocity.Binders;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

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

        public override Expression Reduce()
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
