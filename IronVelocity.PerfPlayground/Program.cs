using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.PerfPlayground
{
    public class Program
    {
        public static void Main()
        {
            var generator = new TemplateCompilation()
            {
                ExecuteTemplate = true,
                SaveDlls = false,
            };


            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var testCases = generator.CreateTemplateTestCases().ToList(); ;

            foreach (var testcase in testCases)
            {
                Console.WriteLine(testcase.TestName);
                try
                {
                    generator.TemplateCompilationTests((string)testcase.Arguments[0], (string)testcase.Arguments[1]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

            }
        }
    }
}
