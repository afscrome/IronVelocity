
namespace IronVelocity.Parser.AST
{
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
}
