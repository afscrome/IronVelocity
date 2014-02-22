using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.Directives
{
    public class MacroScope : IScope
    {
        private readonly IDictionary<string, Expression> _arguments = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, Expression> Arguments { get { return _arguments; } }

        public Expression GetVariable(string name)
        {
            Expression result;

            return _arguments.TryGetValue(name, out result)
                ? result
                : null;
        }
    }
}
