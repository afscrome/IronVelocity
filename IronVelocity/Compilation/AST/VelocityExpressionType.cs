using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Variable
    }
}
