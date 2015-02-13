using NVelocity.Runtime.Parser.Node;
using System;

namespace IronVelocity.Compilation.AST
{
    public abstract class Directive : VelocityExpression
    {
        public override Type Type { get { return typeof(void); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.Directive; } }
    }

}
