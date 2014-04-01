using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Diagnostics;
using IronVelocity.Compilation;
using System.Reflection;
using System.Numerics;

namespace IronVelocity.Binders
{
    public class VelocityBinaryLogicalOperationBinder : BinaryOperationBinder
    {
        public VelocityBinaryLogicalOperationBinder(ExpressionType type)
            : base(type)
        {
        }


        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            if (!arg.HasValue)
                Defer(arg);

            throw new NotImplementedException();
        }
    }

}
