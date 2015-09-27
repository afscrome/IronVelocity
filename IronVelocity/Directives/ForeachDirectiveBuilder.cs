using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Directives
{
    public class ForeachDirectiveBuilder : CustomDirectiveBuilder
    {
        public override bool IsBlockDirective => true;
        public override string Name => "foreach";

        public override Expression Build(IReadOnlyList<Expression> arguments, Expression body)
        {
            return new ForeachDirective(arguments[0], arguments[2], body);
        }
    }
}
