using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class ObjectArrayExpression : VelocityExpression
    {
        public IReadOnlyList<Expression> Values { get; private set; }

        public override Type Type { get { return typeof(IList<object>); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.ObjectArray; } }

        public ObjectArrayExpression(SourceInfo sourceInfo, IReadOnlyList<Expression> args)
        {
            SourceInfo = sourceInfo;
            Values = args;
        }

        public override Expression Reduce()
        {
            var objValues = Values.Select(x => VelocityExpressions.ConvertIfNeeded(x, typeof(object)));
            return Expression.New(MethodHelpers.ListConstructorInfo, Expression.NewArrayInit(typeof(object), objValues));
        }

        public ObjectArrayExpression Update(IReadOnlyList<Expression> arguments)
        {
            if (arguments == Values)
                return this;

            return new ObjectArrayExpression(SourceInfo, arguments);
        }
    }
}
