using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class ObjectArrayExpression : VelocityExpression
    {
        public IImmutableList<Expression> Values { get; }

        public override Type Type => typeof(IList<object>);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.ObjectArray;

        public ObjectArrayExpression(SourceInfo sourceInfo, IImmutableList<Expression> args)
        {
            SourceInfo = sourceInfo;
            Values = args;
        }

        public override Expression Reduce()
        {
            var objValues = Values.Select(x => VelocityExpressions.ConvertIfNeeded(x, typeof(object)));
            return Expression.New(MethodHelpers.ListConstructorInfo, Expression.NewArrayInit(typeof(object), objValues));
        }

        public ObjectArrayExpression Update(IImmutableList<Expression> arguments)
        {
            if (arguments == Values)
                return this;

            return new ObjectArrayExpression(SourceInfo, arguments);
        }
    }
}
