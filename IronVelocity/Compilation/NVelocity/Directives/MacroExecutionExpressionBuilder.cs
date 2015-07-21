using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public class MacroExecutionExpressionBuilder : DirectiveExpressionBuilder
    {
        private readonly LambdaExpression _macro;
        private readonly string _name;

        public override string Name { get { return _name; } }
        public override Type NVelocityDirectiveType { get { return typeof(Macro); } }

        public MacroExecutionExpressionBuilder(string name, LambdaExpression macro)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentOutOfRangeException(nameof(name));

            if (macro == null)
                throw new ArgumentNullException(nameof(macro));

            _name = name;
            _macro = macro;
        }

        public override Expression Build(ASTDirective node, NVelocityNodeToExpressionConverter builder)
        {
            if (node.DirectiveName != Name)
                throw new ArgumentOutOfRangeException(nameof(node));

            if (_macro.Parameters.Count != node.ChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(node));

            var arguments = new Expression[node.ChildrenCount];

            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[i] = VelocityExpressions.ConvertIfNeeded(builder.Operand(node.GetChild(i)), typeof(object));
            }

            return Expression.Invoke(_macro, arguments);
        }

    }
}
