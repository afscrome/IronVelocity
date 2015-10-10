using IronVelocity.Compilation;
using IronVelocity.Directives;
using IronVelocity.Parser;
using IronVelocity.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace IronVelocity.PerfPlayground
{
    [TestFixture(Category = "Performance")]
    [Explicit]
    public class TemplateCompilation
    {
        private const string IldasmPath = "C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.1A\\bin\\NETFX 4.5.1 Tools\\ildasm.exe";

        public bool Compile { get; set; } = false;
        public bool SaveDlls { get; set; } = false;
        public bool SaveIl { get; set; } = false;
        public bool ExecuteTemplate { get; set; } = false;
        public string OutputDir { get; set; } = "CompiledTemplates";
        public ICollection<string> TemplateDirectories { get; } = new List<string>();
        public string TestNamePrefix { get; set; } = "Compilation";
        public ICollection<string> BlockDirectives { get; } = new List<string>();

        [TestFixtureSetUp]
        public void SetUp()
        {
            TemplateDirectories.Add("../../templates/");
            TemplateDirectories.Add("../../../IronVelocity.Tests/Regression/templates/");

            if (SaveDlls || SaveIl)
            {
                OutputDir = TestNamePrefix + DateTime.Now.ToString("yyy-MM-dd_HH-mm-ss") + "\\";

                if (!Directory.Exists(OutputDir))
                    Directory.CreateDirectory(OutputDir);
            }
        }

        [TestCaseSource("CreateTemplateTestCases")]
        public void TemplateCompilationTests(string path, string assemblyName)
        {
            using (var file = File.OpenRead(path))
            {
                var antlrDirectives = BlockDirectives
                    .Select(x => new AntlrBlockDirectiveBuilder(x))
                    .ToList<CustomDirectiveBuilder>();
                antlrDirectives.Add(new ForeachDirectiveBuilder());
                var parser = new AntlrVelocityParser(antlrDirectives, null);

                var expressionTree = parser.Parse(file, assemblyName);
                if (Compile)
                {
                    AssemblyBuilder assemblyBuilder = null;
                    VelocityCompiler compiler;

                    if (SaveDlls || SaveIl)
                    {
                        var diskCompiler = new DiskCompiler(new AssemblyName(assemblyName), OutputDir);
                        assemblyBuilder = diskCompiler.AssemblyBuilder;
                        compiler = diskCompiler;
                    }
                    else
                    {
                        compiler = new VelocityCompiler(null);
                    }

                    var result = compiler.CompileWithSymbols(expressionTree, assemblyName, true, path);

                    if (ExecuteTemplate)
                    {
                        var context = new VelocityContext();
                        using (var writer = new StringWriter())
                        {
                            var output = new VelocityOutput(writer);
                            result(context, output);
                        }
                    }

                    if (SaveDlls || SaveIl)
                    {
                        var dllName = assemblyName + ".dll";
                        assemblyBuilder.Save(dllName);

                        if (SaveIl)
                        {
                            var assemblyPath = Path.Combine(OutputDir, dllName);
                            var ilPath = assemblyPath.Replace(".dll", ".il");
                            var startInfo = new ProcessStartInfo(IldasmPath)
                            {
                                Arguments = $"\"{assemblyPath}\" /item:{assemblyName} /linenum /source /out:\"{ilPath}\"",
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                WindowStyle = ProcessWindowStyle.Hidden
                            };
                            Process.Start(startInfo);
                        }
                    }
                }
            }
        }

        public IEnumerable<TestCaseData> CreateTemplateTestCases()
        {
            foreach (var path in TemplateDirectories)
            {
                var directory = new DirectoryInfo(path);
                if (directory.Exists)
                {
                    foreach (var file in directory.GetFiles("*.vm", SearchOption.AllDirectories))
                    {
                        var relativePath = file.FullName.Replace(directory.FullName, "");
                        var assemblyName = relativePath.Replace(Path.DirectorySeparatorChar, '_').Replace(".vm", "");
                        yield return new TestCaseData(file.FullName, assemblyName)
                           .SetName(TestNamePrefix + " " + relativePath);
                    }
                }
            }
        }

        /// <summary>
        /// Helper compiler to allow 
        /// </summary>
        private class DiskCompiler : VelocityCompiler
        {
            public AssemblyBuilder AssemblyBuilder { get; }
            private readonly AssemblyName _assemblyName;
            public DiskCompiler(AssemblyName name, string outputDir)
                : base(null)
            {
                _assemblyName = name;
                AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.RunAndSave, outputDir);
            }

            protected override ModuleBuilder CreateModuleBuilder(bool debugMode)
            {
                var name = _assemblyName.Name;
                var moduleBuilder = AssemblyBuilder.DefineDynamicModule(name, name + ".dll", true);

                if (debugMode)
                {
                    AddDebugAttributes(AssemblyBuilder, moduleBuilder);
                }

                return moduleBuilder;
            }
        }

        private class AntlrBlockDirectiveBuilder : CustomDirectiveBuilder
        {
            public AntlrBlockDirectiveBuilder(string name)
            {
                Name = name;
            }

            public override bool IsBlockDirective => true;
            public override string Name { get; }

            public override Expression Build(IReadOnlyList<Expression> arguments, Expression body)
                => body;
        }
    }

}
