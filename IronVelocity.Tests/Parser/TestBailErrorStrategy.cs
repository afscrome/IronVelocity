using Antlr4.Runtime;
using IronVelocity.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser
{
    public class TestBailErrorStrategy : BailErrorStrategy
    {
        private readonly string _input;
        public TestBailErrorStrategy(string input)
        {
            _input = input;
        }

        public override void Recover(Antlr4.Runtime.Parser recognizer, RecognitionException e)
        {
            PrintTokens(_input);
            base.Recover(recognizer, e);
        }

        public override IToken RecoverInline(Antlr4.Runtime.Parser recognizer)
        {
            PrintTokens(_input);
            return base.RecoverInline(recognizer);
        }


        public static void PrintTokens(string input)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);

            foreach (var token in lexer.GetAllTokens())
            {
                Console.WriteLine(token);
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(token);
                }
            }
        }
    }
}
