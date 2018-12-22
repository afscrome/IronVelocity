using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace IronVelocity.Directives
{
    public abstract class CustomDirectiveBuilder
    {
        public abstract string Name { get; }
        public abstract bool IsBlockDirective { get; }

        public abstract Expression Build(IImmutableList<Expression> arguments, Expression body);
    }
}
