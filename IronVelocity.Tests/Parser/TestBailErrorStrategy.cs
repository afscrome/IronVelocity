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
        private readonly int? _lexerMode;
        public TestBailErrorStrategy(string input, int? lexerMode)
        {
            _input = input;
            _lexerMode = lexerMode;
        }

        public override void Recover(Antlr4.Runtime.Parser recognizer, RecognitionException e)
        {
            PrintTokens(_input, _lexerMode);
            base.Recover(recognizer, e);
        }

        public override IToken RecoverInline(Antlr4.Runtime.Parser recognizer)
        {
            PrintTokens(_input, _lexerMode);
            return base.RecoverInline(recognizer);
        }


        public static void PrintTokens(string input, int? lexerMode = null)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            if (lexerMode.HasValue)
                lexer.Mode(lexerMode.Value);

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
