
namespace IronVelocity.Compilation.AST
{
    public enum VelocityExpressionType
    {
        CoerceToBoolean,
        Comparison,
        CustomDirective,
        Dictionary,
        DictionaryString,
        Directive,
        Foreach,
        GlobalVariable,
        IntegerRange,
        InterpolatedString,
        Mathematical,
        MethodInvocation,
        ObjectArray,
        PropertyAccess,
        Reference,
        RenderableReference,
        RenderableExpression,
        RenderedBlock,
        SetDirective,
        SetMember,
        TemplatedForeach,
        TemporaryVariableScope,
        Variable
    }
}
