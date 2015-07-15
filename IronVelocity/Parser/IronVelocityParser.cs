using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Compilation;

namespace IronVelocity.Parser
{
    public class IronVelocityParser : IParser
    {
        private readonly VelocityExpressionConverter _converter = new VelocityExpressionConverter();

        public Expression<VelocityTemplateMethod> Parse(string input, string name)
        {
            //throw new NotImplementedException();
            var parser = new VelocityParser(input);
            var log = TemplateGenerationEventSource.Log;

            log.ParseStart(name);
            var tree = parser.Parse();
            log.ParseStop(name);
            if (tree == null)
                throw new InvalidProgramException("Unable to parse ast");

            log.ConvertToExpressionTreeStart(name);
            var expressionTree = _converter.Visit(tree);
            log.ConvertToExpressionTreeStop(name);

            return Expression.Lambda<VelocityTemplateMethod>(expressionTree, name, new[] { Constants.InputParameter, Constants.OutputParameter });
        }
    }
}
