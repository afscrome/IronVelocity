using IronVelocity.Compilation.Directives;
using Microsoft.Scripting;
using NVelocity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Scripting
{
    public class VelocityCompilerOptions : CompilerOptions
    {
        public VelocityCompilerOptions(IDictionary<Type, DirectiveExpressionBuilder> directiveHandlers)
        {
            Runtime = new RuntimeInstance();

            var properties = new Commons.Collections.ExtendedProperties();
            properties.AddProperty("input.encoding", "utf-8");
            properties.AddProperty("output.encoding", "utf-8");
            properties.AddProperty("velocimacro.permissions.allow.inline", "false");
            properties.AddProperty("runtime.log.invalid.references", "false");
            properties.AddProperty("runtime.log.logsystem.class", typeof(NVelocity.Runtime.Log.NullLogSystem).AssemblyQualifiedName.Replace(",", ";"));
            properties.AddProperty("parser.pool.size", 0);
            ArrayList userDirectives = new ArrayList();

            if (directiveHandlers != null)
            {
                foreach (var directive in directiveHandlers)
                {
                    userDirectives.Add(directive.Key.AssemblyQualifiedName);
                }
                properties.AddProperty("userdirective", userDirectives);
            }
            Runtime.Init(properties);
            DirectiveHandlers = new Dictionary<Type, DirectiveExpressionBuilder>(directiveHandlers);


        }

        public IReadOnlyDictionary<Type, DirectiveExpressionBuilder> DirectiveHandlers { get; }
        public IReadOnlyDictionary<string, Type> StaticGlobalTypes { get; }
        public bool OutputAsText { get; set; }
        public bool DebugSymbols { get; set; }
        public bool StaticTypeGlobals { get { return DirectiveHandlers != null && DirectiveHandlers.Any(); } }
        public RuntimeInstance Runtime { get; }
    }
}
