using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace IronVelocity.Directives
{
    public class ForeachDirectiveBuilder : CustomDirectiveBuilder
    {
        public override bool IsBlockDirective => true;
        public override string Name => "foreach";

        public override Expression Build(IImmutableList<Expression> arguments, Expression body)
            => new ForeachDirective(arguments[0], arguments[2], body);
    }
}
