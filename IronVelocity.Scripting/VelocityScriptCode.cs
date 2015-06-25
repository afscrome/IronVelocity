using IronVelocity.Compilation;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using System;

namespace IronVelocity.Scripting
{
    public class VelocityScriptCode : ScriptCode
    {
        private readonly VelocityTemplateMethod _method;

        public VelocityScriptCode(SourceUnit sourceUnit, VelocityRootExpression ast)
            :base(sourceUnit)
        {


            _method = CompilerHelpers.LightCompile(ast.GetLambda());

        }

        public override object Run(Scope scope)
        {
            throw new NotImplementedException();
        }

    }
}
