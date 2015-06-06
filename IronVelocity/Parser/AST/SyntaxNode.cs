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
        public bool IsFormal { get; set; }
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



    public class IntegerNode : ExpressionNode
    {
        public int Value { get; set; }
    }
    public class FloatingPointNode : ExpressionNode
    {
        public float Value { get; set; }
    }

    public class StringNode : ExpressionNode
    {
        public string Value { get; set; }
        public bool IsInterpolated { get; set; }
    }

    public class BooleanNode : ExpressionNode
    {
        public static readonly BooleanNode True = new BooleanNode(true);
        public static readonly BooleanNode False = new BooleanNode(false);

        public bool Value { get; private set; }

        public BooleanNode(bool value)
        {
            Value = value;
        }
    }

    public class WordNode : ExpressionNode
    {
        public string Name { get; set; }
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
        public BinaryOperation Operation { get; set; }
    }

    public enum BinaryOperation
    {
        Range
    }

    /*
    public class Method : ReferenceInnerNode
    {
        public ReferenceInnerNode Target { get; private set; }
    }*/
}
