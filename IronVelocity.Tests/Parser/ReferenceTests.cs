using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser
{
    class ReferenceTests : ParserTestBase
    {
        [TestCase("$foo")]
        [TestCase("$!bar")]
        [TestCase("${foo}")]
        [TestCase("$!{bar}")]
        [TestCase("$CAPITAL")]
        [TestCase("$MiXeDCaSe")]
        [TestCase("$WithNumbers")]
        public void VariableReferences(string input)
        {
            var result = CreateParser(input).template();
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            Assert.That(flattened, Has.Exactly(1).InstanceOf<VelocityParser.ReferenceContext>());
        }

        [TestCase("$foo.dog", "dog")]
        [TestCase("$!bar.cat", "cat")]
        [TestCase("${foo.fish}", "fish")]
        [TestCase("$!{bar.bear}", "bear")]
        public void Property(string input, string propertyName)
        {
            var result = CreateParser(input).template();
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            var reference = flattened.OfType<VelocityParser.ReferenceContext>().Single();
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var property = flattened.OfType<VelocityParser.Property_invocationContext>().Single();
            Assert.That(property.GetText(), Is.EqualTo(propertyName));
        }


        [TestCase("$foo.dog()", "dog()")]
        [TestCase("$!bar.cat()", "cat()")]
        [TestCase("${foo.fish()}", "fish()")]
        [TestCase("$!{bar.bear()}", "bear()")]
        [TestCase("$hello.world( )", "world( )")]
        [TestCase("$hello.world(\t)", "world(\t)")]
        [TestCase("$hello.world( \t  \t\t )", "world( \t  \t\t )")]
        public void ZeroArgumentMethod(string input, string methodText)
        {
            var result = CreateParser(input).template();
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            var reference = flattened.OfType<VelocityParser.ReferenceContext>().Single();
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var property = flattened.OfType<VelocityParser.Method_invocationContext>().Single();
            Assert.That(property.GetText(), Is.EqualTo(methodText));
        }

        [TestCase("true")]
        [TestCase("false")]
        [TestCase("123")]
        [TestCase("-456")]
        [TestCase("3.14")]
        [TestCase("-2.718")]
        [TestCase("'simple string'")]
        [TestCase("\"Interpolated String\"")]
        [TestCase("$foo")]
        [TestCase("$!foo")]
        [TestCase("${foo}")]
        [TestCase("$!{foo}")]
        [TestCase("$foo.Bar")]
        [TestCase("$foo.Baz()")]
        [TestCase("[]")]
        [TestCase("[ ]")]
        //TODO: the following test can't test for methods with arguments, non-empty lists or ranges
        public void OneArgumentMethod(string argument)
        {
            var input = $"$obj.method({argument})";
            var result = CreateParser(input).template();
            var flattened = FlattenParseTree(result);

            //Assert.That(flattened, Has.Exactly(1).InstanceOf<VelocityParser.Method_invocationContext>());
            var arg = flattened.OfType<VelocityParser.ArgumentContext>().Single();

            Assert.That(arg.GetText(), Is.EqualTo(argument));
        }

        [Test]
        public void TwoArguments()
        {
            var input = "$variable.method(123, true)";
            var result = CreateParser(input).template();
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.Exactly(1).InstanceOf<VelocityParser.Method_invocationContext>());
            Assert.That(flattened, Has.Exactly(2).InstanceOf<VelocityParser.ArgumentContext>());
        }

        public void TwoReferenceswithTextInBetween(string reference1, string text, string reference2)
        {
            var input = reference1 + text + reference2;
            var result = CreateParser(input).template();

            var flattened = FlattenParseTree(result);

            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();
            Assert.That(textNode.GetText(), Is.EqualTo(text));

            var references = flattened.OfType<VelocityParser.ReferenceContext>()
                .Select(x => x.GetText())
                .ToList();

            Assert.That(references, Contains.Item(reference1));
            Assert.That(references, Contains.Item(reference2));
        }
    }
}
