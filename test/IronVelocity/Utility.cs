using System;

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
