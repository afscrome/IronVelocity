using IronVelocity;
using IronVelocity.Compilation;
using IronVelocity.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests
{
    public static class Utility
    {
        public static string GetName()
        {
            var testContext = NUnit.Framework.TestContext.CurrentContext;
            return testContext == null
                ? "TestExpression"
                : testContext.Test.Name;
        }

        public static string NormaliseLineEndings(string output)
        {
            return output.Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }
    }
}
