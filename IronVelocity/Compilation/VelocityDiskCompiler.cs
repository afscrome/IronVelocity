using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace IronVelocity.Compilation
{
    public class VelocityDiskCompiler : VelocityCompiler
    {
        private const string IldasmPath = "C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.1A\\bin\\NETFX 4.5.1 Tools\\ildasm.exe";

        private AssemblyBuilder _assemblyBuilder { get; }
        private readonly AssemblyName _assemblyName;
        private readonly string _outputDir;

        public VelocityDiskCompiler(AssemblyName name, string outputDir)
            : base(null)
        {
            _assemblyName = name;
            _outputDir = outputDir;
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.RunAndSave, outputDir);
        }

        protected override ModuleBuilder CreateModuleBuilder(bool debugMode)
        {
            var name = _assemblyName.Name;
            var moduleBuilder = _assemblyBuilder.DefineDynamicModule(name, name + ".dll", true);

            if (debugMode)
            {
                AddDebugAttributes(_assemblyBuilder, moduleBuilder);
            }

            return moduleBuilder;
        }

        public void SaveDll()
        {
            var dllName = _assemblyName.Name + ".dll";
            _assemblyBuilder.Save(dllName);
        }

        public void SaveIl()
        {
            var assemblyPath = Path.Combine(_outputDir, _assemblyName.Name + ".dll");
            var ilPath = assemblyPath.Replace(".dll", ".il");
            var startInfo = new ProcessStartInfo(IldasmPath)
            {
                Arguments = $"\"{assemblyPath}\" /item:{_assemblyName.Name} /linenum /source /out:\"{ilPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(startInfo);
        }
    }
}
