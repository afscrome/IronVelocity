using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class UnrecognisedDirective : Directive
    {
        private readonly string _literal;

        public UnrecognisedDirective(INode node, VelocityExpressionBuilder builder) :base(node, builder)
        {
            _literal = node.Literal;
        }

        public override Expression Reduce()
        {
            return Expression.Constant(_literal);
        }

        public override Type Type
        {
            get { return typeof(string);}
        }
    }
}
