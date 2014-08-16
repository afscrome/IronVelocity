using IronVelocity.Compilation.Directives;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using NVelocity.Runtime;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IronVelocity.Scripting
{
    public class IronVelocityLanguageContext : LanguageContext
    {
        private static readonly Guid _vendorId;
        private static readonly Guid _languageId;
        private readonly RuntimeInstance _runtimeService;
        private readonly VelocityCompilerOptions _options;
        private readonly Scope _globals;
        private static readonly IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<Type, DirectiveExpressionBuilder>()
        {
            {typeof(Foreach), new ForeachDirectiveExpressionBuilder()},
            {typeof(Literal), new LiteralDirectiveExpressionBuilder()},
        };

        public IronVelocityLanguageContext(ScriptDomainManager manager, IDictionary<string, object> options)
            : base(manager)
        {
            _globals = manager.Globals;
            _options = new VelocityCompilerOptions(_directiveHandlers);
        }



        public override Guid VendorGuid { get { return _vendorId; } }
        public override Guid LanguageGuid { get { return _languageId; } }
        public override Version LanguageVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public override Microsoft.Scripting.CompilerOptions GetCompilerOptions()
        {
            return _options;
        }

        public override Microsoft.Scripting.CompilerOptions GetCompilerOptions(Scope scope)
        {
            return _options;
        }


        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
        {
            var velocityOptions = (VelocityCompilerOptions)options;

            var parser = velocityOptions.Runtime.CreateNewParser();
            using (var reader = sourceUnit.GetReader())
            {
                var nVelocityAst = parser.Parse(reader, null) as ASTprocess;

                var ast = new VelocityRootExpression(nVelocityAst, velocityOptions, "todo");

                return new VelocityScriptCode(sourceUnit, ast);
            }
        }


    }



}
