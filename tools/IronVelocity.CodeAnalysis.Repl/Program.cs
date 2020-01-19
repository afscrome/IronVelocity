using IronVelocity.CodeAnalysis;
using IronVelocity.CodeAnalysis.Syntax;
using IronVelocity.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var text = new SourceText(line);
            var lexer = new Lexer(text);
            var tokens = lexer.ReadAllTokens();

            if (lexer.Diagnostics.Any())
            {
                PrintErrors(text, lexer.Diagnostics);
                PrintTokens(tokens);
                return;
            }

            if (_printTokens)
            {
                PrintTokens(tokens);
            }


            var parser = new Parser(tokens);
            var syntaxTree = parser.Parse();
            if (syntaxTree.Diagnostics.Any())
            {
                PrintErrors(text, syntaxTree.Diagnostics);
                PrintParseTree(syntaxTree);
                return;
            }

            if (_printTree)
            {
                PrintParseTree(syntaxTree);
            }

            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate();

            if (result.Diagnostics.Any())
            {
                PrintErrors(text, result.Diagnostics);
                return;
            }

            WriteLineToConsole(ConsoleColor.DarkGreen, result.Value);
        }

        private static void PrintErrors(SourceText text, IEnumerable<Diagnostic> diagnostics)
        {
            foreach (var diagnostic in diagnostics)
            {
                var startPosition = text.GetLineAndCharacterPosition(diagnostic.Span.Start);
                var line = text.Lines[startPosition.Line];

                var linePrefix = text.Substring(line.Start, startPosition.Character);
                var lineProblem = text.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                var lineSuffix = text.Substring(diagnostic.Span.End, line.Length - diagnostic.Span.End);

                Console.WriteLine();
                WriteLineToConsole(ConsoleColor.Red, $"Line: {startPosition.Line + 1} Char: {startPosition.Character + 1} {diagnostic.Message}");
                Console.Write("    ");
                Console.Write(linePrefix);
                WriteToConsole(ConsoleColor.DarkRed, lineProblem);
                Console.WriteLine(lineSuffix);
            }

            Console.WriteLine();
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

        private static void WriteLineToConsole(ConsoleColor color, object? text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

    }
}
