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
        private readonly IReadOnlyDictionary<string, object> _globals;


        public VelocityRuntime(IParser parser, IDictionary<string, object> globals)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            _parser = parser;


            if (globals == null)
                _globals = new Dictionary<string, object>();
            else
                _globals = new Dictionary<string, object>(globals, StringComparer.OrdinalIgnoreCase);

            var globalsTypeMap = _globals.ToDictionary(x => x.Key, x => x.Value.GetType());
            _compiler = new VelocityCompiler(globalsTypeMap);

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
