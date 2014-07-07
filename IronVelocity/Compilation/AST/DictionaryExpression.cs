using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class DictionaryExpression : VelocityExpression
    {
        public IDictionary<string, Expression> Values { get; private set; }

        public DictionaryExpression(IDictionary<string, Expression> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            Values = values;
        }

        public override Expression Reduce()
        {
            return VelocityExpressions.Dictionary(Values);
        }

        public override Type Type { get { return typeof(RuntimeDictionary); } }
    }
}
