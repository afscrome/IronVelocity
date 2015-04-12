using IronVelocity.Compilation;
using IronVelocity.Compilation.AST;
using IronVelocity.Compilation.Directives;
using NVelocity.Runtime;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity
{
    public class TestLog : NVelocity.Runtime.Log.ILogSystem
    {
        public void Init(IRuntimeServices rs)
        {
        }

        public void LogVelocityMessage(NVelocity.Runtime.Log.LogLevel level, string message)
        {
            Console.WriteLine("[{0}] message");
        }
    }


    public class VelocityRuntime
    {
        private readonly IParser _parser;
        private readonly VelocityCompiler _compiler;
        private readonly IReadOnlyDictionary<string, object> _globals;


        public VelocityRuntime(IParser parser, IDictionary<string, object> globals)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            _parser = parser;


            if (globals == null)
                _globals = new Dictionary<string, object>();
            else
                _globals = new Dictionary<string, object>(globals, StringComparer.OrdinalIgnoreCase);

            var globalsTypeMap = _globals.ToDictionary(x => x.Key, x => x.Value.GetType());
            _compiler = new VelocityCompiler(globalsTypeMap);

        }


        public VelocityTemplateMethod CompileTemplate(string input, string typeName, string fileName, bool debugMode)
        {
            var template = _parser.Parse(input, typeName);
            return _compiler.CompileWithSymbols(template, typeName, debugMode, fileName);
        }

    }
}
