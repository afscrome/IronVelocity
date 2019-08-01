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

                var lexer = new Lexer(line);
                var tokens = GetTokens(lexer);

                PrintTokens(tokens);

                if (lexer.Diagnostics.Any())
                {
                    PrintErrors(lexer.Diagnostics);
                }
                else
                {
                    Console.WriteLine();

                    var parser = new IronVelocity.CodeAnalysis.Syntax.Parser(tokens);
                    var syntaxTree = parser.Parse();
                    PrintParseTree(syntaxTree);
                    if (parser.Diagnostics.Any())
                    {
                        PrintErrors(parser.Diagnostics);
                    }
                }
                Console.WriteLine();
                Console.WriteLine();

            }

        }
        private static ImmutableList<SyntaxToken> GetTokens(Lexer lexer)
        {
            var builder = ImmutableList.CreateBuilder<SyntaxToken>();

            while (true)
            {
                var token = lexer.NextToken();
                builder.Add(token);
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;
            }

            return builder.ToImmutable();
        }

        private static void PrintErrors(IEnumerable<string> errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            foreach(var error in errors)
            {
                Console.WriteLine(error);
            }
            Console.ResetColor();
        }

        private static void PrintTokens(IEnumerable<SyntaxToken> tokens)
        {
            WriteToConsole("Tokens:", ConsoleColor.DarkGray);
            Console.WriteLine();

            foreach(var token in tokens)
            {
                WriteToConsole(token.Position.ToString().PadLeft(3,' ') + ": ", ConsoleColor.DarkGray);
                WriteToConsole("<" + token.Kind + ">", ConsoleColor.DarkCyan);
                WriteToConsole(" ", ConsoleColor.DarkGray);
                WriteToConsole("'" + token.Text + "'", ConsoleColor.DarkMagenta);

                if (token.Value != null)
                {
                    WriteToConsole(" " + token.Value, ConsoleColor.DarkGreen);
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        private static void PrintParseTree(SyntaxTree syntaxTree)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Parse Tree:");
            ParseTreePrinter.PrettyPrint(syntaxTree.Root, Console.Out, firstLinePrefix: "    ", furtherLinePrefix: "    ");

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
