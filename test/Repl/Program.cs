using IronVelocity.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Repl
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                var tokens = Lex(line);
                PrintTokens(tokens);
                Console.WriteLine();

                var parser = new IronVelocity.CodeAnalysis.Syntax.Parser(tokens);
                var expression = parser.ParseExpression();
                PrintParseTree(expression);
                Console.WriteLine();
                Console.WriteLine();

            }

        }
        private static ImmutableList<SyntaxToken> Lex(string text)
        {
            var lexer = new Lexer(text);

            var builder = ImmutableList.CreateBuilder<SyntaxToken>();

            while (true)
            {
                var token = lexer.NextToken();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;
                builder.Add(token);
            }

            return builder.ToImmutable();
        }

        private static void PrintTokens(IEnumerable<SyntaxToken> tokens)
        {
            WriteToConsole("Tokens:", ConsoleColor.DarkGray);
            Console.WriteLine();

            foreach(var token in tokens)
            {
                WriteToConsole(token.Position.ToString().PadLeft(3,' ') + ": ", ConsoleColor.DarkGray);
                WriteToConsole(token.Kind, ConsoleColor.DarkCyan);
                WriteToConsole(": ", ConsoleColor.Gray);
                WriteToConsole("'" + token.Text + "'", ConsoleColor.DarkMagenta);

                if (token.Value != null)
                {
                    WriteToConsole(" " + token.Value, ConsoleColor.DarkGreen);
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        private static void PrintParseTree(ExpressionSyntax expression)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Parse Tree:");
            ParseTreePrinter.PrettyPrint(expression, Console.Out, firstLinePrefix: "    ", furtherLinePrefix: "    ");

            Console.WriteLine();
            Console.ResetColor();
        }

        private static void WriteToConsole(object text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
        }
    }
}
