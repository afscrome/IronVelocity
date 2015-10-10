using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IronVelocity
{
    public class VelocityRuntime
    {
        private readonly IParser _parser;
        private readonly VelocityCompiler _compiler;

        public VelocityRuntime(IParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            _parser = parser;

            _compiler = new VelocityCompiler();
        }


        public VelocityTemplateMethod CompileTemplate(Stream input, string typeName, string fileName, bool debugMode)
        {
            var template = _parser.Parse(input, typeName);
            return _compiler.CompileWithSymbols(template, typeName, debugMode, fileName);
        }

        public VelocityTemplateMethod CompileTemplate(string input, string typeName, string fileName, bool debugMode)
        {
            var template = _parser.Parse(input, typeName);
            return _compiler.CompileWithSymbols(template, typeName, debugMode, fileName);
        }

    }
}
