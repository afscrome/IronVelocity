using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.Directives
{
    public class MacroExecutionExpressionBuilder : DirectiveExpressionBuilder
    {
        private readonly LambdaExpression _macro;
        private readonly string _name;

        public override string Name { get { return _name; } }

        public MacroExecutionExpressionBuilder(string name, LambdaExpression macro)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentOutOfRangeException("name");

            if (macro == null)
                throw new ArgumentNullException("macro");

            _name = name;
            _macro = macro;
        }

        public override Expression Build(ASTDirective node, NVelocityExpressions builder)
        {
            if (node.DirectiveName != Name)
                throw new ArgumentOutOfRangeException("node");

            if (_macro.Parameters.Count != node.ChildrenCount)
                throw new ArgumentOutOfRangeException("node");

            var arguments = new Expression[node.ChildrenCount];

            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[i] = VelocityExpressions.ConvertIfNeeded(builder.Operand(node.GetChild(i)), typeof(object));
            }

            return Expression.Invoke(_macro, arguments);
        }

        private static ICollection<string> getASTAsStringArray(INode rootNode)
        {
            Token lastToken = rootNode.LastToken;
            var  arrayList = new List<string>();
            Token t;
            for (t = rootNode.FirstToken; t != lastToken; t = t.Next)
                arrayList.Add(NodeUtils.tokenLiteral(t));
            arrayList.Add(NodeUtils.tokenLiteral(t));
            return arrayList;
        }

    }
}
