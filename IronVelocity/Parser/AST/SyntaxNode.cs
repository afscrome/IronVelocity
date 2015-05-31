using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public abstract class SyntaxNode
    {

    }

    public abstract class ExpressionNode : SyntaxNode
    {

    }

    public class ArgumentsNode : SyntaxNode
    {
        public IReadOnlyList<ExpressionNode> Arguments { get; set; }
    }

    public class ReferenceNode : ExpressionNode
    {
        public bool IsSilent { get; set; }
        public bool IsFormal { get;  set; }
        public SyntaxNode Value { get; set; }
    }

    public abstract class ReferenceInnerNode : SyntaxNode
    {
        public string Name { get; set; }
    }
    public class Variable : ReferenceInnerNode
    {
    }

    public class Property : ReferenceInnerNode
    {
        public ReferenceInnerNode Target { get; set; }
    }

    public class Method : ReferenceInnerNode
    {
        public ReferenceInnerNode Target { get; set; }
        public ArgumentsNode Arguments { get; set; }
    }



    public class NumericNode : ExpressionNode
    {
        public string Value { get; set; }
    }


    /*
    public class Method : ReferenceInnerNode
    {
        public ReferenceInnerNode Target { get; private set; }
    }*/
}
