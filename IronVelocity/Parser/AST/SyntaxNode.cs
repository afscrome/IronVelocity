using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public ReferenceInnerNode Value { get; set; }
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


    public class ListExpressionNode : ExpressionNode
    {
        public ListExpressionNode(ExpressionNode singleValue)
        {
            Values = new[] { singleValue };
        }

        public ListExpressionNode(IReadOnlyList<ExpressionNode> values)
        {
            Values = values;
        }

        public IReadOnlyList<ExpressionNode> Values { get; private set; }
    }


    /// <summary>
    /// The most significant 16 bits represent the precedence of the operator
    /// The last 16 bits are used to differentiate operators with the same precedence
    /// </summary>
    public enum BinaryOperation : uint
    {
        //Multiplicative
        Multiplication = 0xF0000000,
        Division = 0xF0000001,
        Modulo = 0xF0000002,
        //Additive
        Adddition = 0xE0000000,
        Subtraction = 0xE0000001,
        //Relational
        LessThan = 0xD0000000,
        GreaterThan = 0xD0000001,
        LessThanOrEqual = 0xD0000002,
        GreaterThanOrEqual = 0xD0000003,
        //Equality
        Equal = 0xC0000000,
        NotEqual = 0xC0000000,
        //BooleanLogic
        And = 0xB0000000,
        Or = 0xA0000000,
        Range = 0x90000000,
    }

    /*
    public class Method : ReferenceInnerNode
    {
        public ReferenceInnerNode Target { get; private set; }
    }*/
}
