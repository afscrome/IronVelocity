using System;

namespace IronVelocity.Compilation.AST
{
    public abstract class Directive : VelocityExpression
    {
        public override Type Type => typeof(void);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.Directive;
    }
}
