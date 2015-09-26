using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser
{
    public abstract class CustomDirective
    {
        public abstract string Name { get; }
        public abstract bool IsBlockDirective { get; }

        public abstract Expression Build(IReadOnlyList<Expression> arguments, Expression body);
    }
}
