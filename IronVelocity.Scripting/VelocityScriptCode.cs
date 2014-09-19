using IronVelocity.Compilation;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using System;
using System.Linq.Expressions;
using System.Text;

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
            var context = new VelocityContext
            {
                {"test" , 123}
            };
            var output = new StringBuilder();
            _method(context, output);

            Console.WriteLine("------------------");
            Console.WriteLine(output.ToString());
            Console.WriteLine("------------------");

            return null;
        }

    }
}
