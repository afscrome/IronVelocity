using IronVelocity.Compilation;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Scripting
{
    public class VelocityScriptCode : ScriptCode
    {
        private readonly VelocityTemplateMethod _method;

        public VelocityScriptCode(SourceUnit sourceUnit, VelocityRootExpression ast)
            :base(sourceUnit)
        {


            var lambda = Expression.Lambda<VelocityTemplateMethod>(ast, sourceUnit.Path, new[] { Constants.InputParameter, ast._builder.OutputParameter});
            _method = CompilerHelpers.LightCompile(lambda);

        }

        public override object Run(Scope scope)
        {
            var context = new VelocityContext();
            var output = new StringBuilder();
            _method(context, output);

            Console.WriteLine("------------------");
            Console.WriteLine(output.ToString());
            Console.WriteLine("------------------");

            return null;
        }

    }
}
