using Antlr4.Runtime;
using IronVelocity.Parser;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;


namespace IronVelocity.Tests.Parser
{
    [Category("Parser")]
    public abstract class ParserTestBase
    {
        protected VelocityParser CreateParser(string input, int? lexerMode = null)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VelocityParser(tokenStream)
            {
                ErrorHandler = new TestBailErrorStrategy(input, lexerMode)
            };

            if (lexerMode.HasValue)
                lexer.Mode(lexerMode.Value);

            return parser;
        }


    }
}
