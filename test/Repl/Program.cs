using IronVelocity.CodeAnalysis;
using IronVelocity.CodeAnalysis.Binding;
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

        private static bool _printTokens = false;
        private static bool _printTree = false;

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

                try
                {
                    switch (line.ToLower())
                    {
                        case "#showtokens":
                            _printTokens = !_printTokens;
                            WriteLineToConsole(ConsoleColor.DarkGray, "Print Tokens: " + _printTokens);
                            break;

                        case "#showtree":
                            _printTree = !_printTree;
                            WriteLineToConsole(ConsoleColor.DarkGray, "Print Tree: " + _printTree);
                            break;

                        case var x when string.IsNullOrWhiteSpace(line):
                            return;

                        default:
                            EvaluateLine(line);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    WriteLineToConsole(ConsoleColor.Red, "Failed to Evaluate:");
                    WriteLineToConsole(ConsoleColor.Red, ex);
                }
                Console.WriteLine();
            }


        }

        private static void EvaluateLine(string line)
        {
            var lexer = new Lexer(line);
            var tokens = lexer.ReadAllTokens();

            if (lexer.Diagnostics.Any())
            {
                PrintErrors(lexer.Diagnostics);
                PrintTokens(tokens);
            }
            else
            {
                if (_printTokens)
                {
                    PrintTokens(tokens);
                }


                var parser = new IronVelocity.CodeAnalysis.Syntax.Parser(tokens);
                var syntaxTree = parser.Parse();
                if (syntaxTree.Diagnostics.Any())
                {
                    PrintErrors(syntaxTree.Diagnostics);
                    PrintParseTree(syntaxTree);
                }
                else
                {
                    if (_printTree)
                    {
                        PrintParseTree(syntaxTree);
                    }

                    var binder = new Binder();

                    var boundExpression = binder.BindExpression(syntaxTree.Root);

                    if (binder.Diagnostics.Any())
                    {
                        PrintErrors(binder.Diagnostics);
                    }
                    else
                    {
                        var evaluator = new Evaluator(boundExpression);
                        WriteLineToConsole(ConsoleColor.DarkGreen, evaluator.Evaluate());
                    }
                }
            }
        }

        private static void PrintErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                WriteLineToConsole(ConsoleColor.Red, error);
            }
        }

        private static void PrintTokens(IEnumerable<SyntaxToken> tokens)
        {
            WriteToConsole(ConsoleColor.DarkGray, "Tokens:");
            Console.WriteLine();

            foreach (var token in tokens)
            {
                WriteToConsole(ConsoleColor.DarkGray, token.Position.ToString().PadLeft(3, ' ') + ": ");
                WriteToConsole(ConsoleColor.DarkCyan, "<" + token.Kind + ">");
                WriteToConsole(ConsoleColor.DarkGray, " ");
                WriteToConsole(ConsoleColor.DarkMagenta, "'" + token.Text + "'");

                if (token.Value != null)
                {
                    WriteToConsole(ConsoleColor.DarkGreen, " " + token.Value);
                }
                Console.WriteLine();
            }
        }

        private static void PrintParseTree(SyntaxTree syntaxTree)
        {
            WriteToConsole(ConsoleColor.DarkGray, "Parse Tree:");
            Console.WriteLine();

            ParseTreePrinter.PrettyPrint(syntaxTree.Root, Console.Out, firstLinePrefix: "    ", furtherLinePrefix: "    ");
            Console.WriteLine();
        }

        private static void WriteToConsole(ConsoleColor color, object text)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        private static void WriteLineToConsole(ConsoleColor color, object text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

    }
}
