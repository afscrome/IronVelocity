using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Method
    {
        [Test]
        public void ParameterlessMethod()
        {
            var input = "#set($x = 123)$x.ToString()";
            var expected = "123";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void MethodWithConstantArgument()
        {
            var input = "$test.test(23)";
            var expected = "23";
            var env = new Dictionary<string, object>();
            env["test"] = new Test();


            Utility.TestExpectedMarkupGenerated(input, expected, env);
        }

        [Test]
        public void MethodWithVariableArgument()
        {
            var input = "$test.test($x)";
            var expected = "54";
            var env = new Dictionary<string, object>();
            env["test"] = new Test();
            env["x"] = 54;

            Utility.TestExpectedMarkupGenerated(input, expected, env);
        }

        [Test]
        public void MethodWithNullReferenceArgument()
        {
            var input = "$test.test($null.ref)";
            var expected = "$test.test($null.ref)";
            var env = new Dictionary<string, object>();
            env["test"] = new Test();


            Utility.TestExpectedMarkupGenerated(input, expected, env);
        }



        [Test]
        public void MethodWithInvalidArgument()
        {
            var input = "#set($x = 'test')$test.test($x.NonExistant)";
            var expected = "$test.test($x.NonExistant)";
            var env = new Dictionary<string, object>();
            env["test"] = new Test();


            Utility.TestExpectedMarkupGenerated(input, expected, env);
        }

        [Test]
        public void MethodWithTwoInvalidArgument()
        {
            var input = "$test.Two($a,$b)";
            var expected = "$test.Two($a,$b)";
            var env = new Dictionary<string, object>();
            env["test"] = new Test();


            Utility.TestExpectedMarkupGenerated(input, expected, env);
        }

        [Test]
        public void MethodWithInvalidStructArgument()
        {
            var input = "$test.OneStruct($a)";
            var expected = "$test.OneStruct($a)";
            var env = new Dictionary<string, object>();
            env["test"] = new Test();

            Utility.TestExpectedMarkupGenerated(input, expected, env);
        }

        [Test]
        public void MethodWithNullReferenceTypeArgument()
        {
            var input = "$test.OneReferenceType($a)";
            var expected = "$test.OneReferenceType($a)";
            var env = new Dictionary<string, object>();
            env["test"] = new Test();

            Utility.TestExpectedMarkupGenerated(input, expected, env);
        }
        public class Test
        {
            public int test(int i)
            {
                return i;
            }

            public string Two(Guid one, Guid two)
            {
                return "boo";
            }

            public string OneStruct(Guid one)
            {
                return "ya";
            }
            public string OneReferenceType(string one)
            {
                return one;
            }

        }

    }
}
