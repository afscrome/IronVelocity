using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public abstract class BinaryExpression : VelocityExpression
    {
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }

        protected BinaryExpression(INode node)
            : base(node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.ChildrenCount != 2)
                throw new NotImplementedException("Expected exactly two children for a binary expression");


            Left = ConversionHelpers.Operand(node.GetChild(0));
            Right = ConversionHelpers.Operand(node.GetChild(1));

        }

    }
}
