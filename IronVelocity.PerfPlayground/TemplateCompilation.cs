using IronVelocity.Compilation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.PerfPlayground
{
    [TestFixture(Category="Performance")]
    [Explicit]
    public class TemplateCompilation
    {

        public TemplateCompilation()
        {
            OutputDir= "CompiledTemplates";
            TemplateDirectories = new[] {"../../templates/", "../../../IronVelocity.Tests/Regression/templates/"};
            SaveDlls = true;
            ExecuteTemplate = false;
        }

        public bool SaveDlls { get; set; }
        public bool ExecuteTemplate { get; set; }
        public string OutputDir { get; set; }
        public ICollection<string> TemplateDirectories {get; set;}

        [TestFixtureSetUp]
        public void SetUp()
        {
            OutputDir = "TemplateCompilation_" + DateTime.Now.ToString("yyy-MM-dd_HH-mm-ss");
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);
        }

        [TestCaseSource("CreateTemplateTestCases")]
        public void TemplateCompilationTests(string path, string assemblyName)
        {
            var template = File.ReadAllText(path);
            var expressionTree = new NVelocityParser(null).Parse(template, assemblyName);
            AssemblyBuilder assemblyBuilder = null;
            VelocityCompiler compiler;

            if (SaveDlls)
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
                var output = new StringBuilder();
                result(context, output);
            }

            if (SaveDlls)
            {
                assemblyBuilder.Save(assemblyName + ".dll");
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
                           .SetName("ILGeneration: " + relativePath);
                    }
                }
            }
        }

        /// <summary>
        /// Helper compiler to allow 
        /// </summary>
        private class DiskCompiler : VelocityCompiler
        {
            public AssemblyBuilder AssemblyBuilder { get; private set; }
            private readonly AssemblyName _assemblyName;
            public DiskCompiler(AssemblyName name, string outputDir)
                : base(null)
            {
                _assemblyName = name;
                AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.RunAndSave, outputDir);
            }

            protected override  ModuleBuilder CreateModuleBuilder(bool debugMode)
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

    }
}
