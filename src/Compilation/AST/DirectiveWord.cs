using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class DirectiveWord : VelocityExpression
    {
        public override VelocityExpressionType VelocityExpressionType
            => VelocityExpressionType.DirectiveWord; 

        public string Name { get; }

        public DirectiveWord(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));

            Name = name;
        }

        public override Expression Reduce()
        {
            throw new NotSupportedException("Directives must not be left to compile");
        }
    }
}
