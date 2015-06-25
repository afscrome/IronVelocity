﻿
namespace IronVelocity.Parser.AST
{
    public class BooleanLiteralNode : ExpressionNode
    {
        public static readonly BooleanLiteralNode True = new BooleanLiteralNode(true);
        public static readonly BooleanLiteralNode False = new BooleanLiteralNode(false);

        public bool Value { get; private set; }

        public BooleanLiteralNode(bool value)
        {
            Value = value;
        }
    }
}
