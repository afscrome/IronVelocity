using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.Directive
{
    public abstract class CustomDirective
    {
        public abstract bool AcceptsParameters { get; }
        public abstract bool IsMultiline { get; }
        public abstract Expression Build(IReadOnlyList<Expression> arguments, Expression body);

    }
}
