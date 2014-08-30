using Microsoft.Scripting.Hosting;

namespace IronVelocity.Scripting
{
    public static class Program
    {
        public static void Main()
        {
            var langSetup = new LanguageSetup(typeof(IronVelocityLanguageContext).AssemblyQualifiedName, "IronVelocity", new[] { "IronVelocity"}, new[] { "vm"});
            var setup = new ScriptRuntimeSetup
            {
                DebugMode = true,
                PrivateBinding = false,
            };
            setup.LanguageSetups.Add(langSetup);
            var runtime = new ScriptRuntime(setup);

            runtime.Globals.SetVariable("test", 123);

            var engine = runtime.GetEngine("IronVelocity");
            var scriptSource = engine.CreateScriptSourceFromFile("c:\\temp\\test.vm");

            var scope = runtime.CreateScope();
            scriptSource.Execute(scope);
        }
    }
}
