
using System;
using System.IO;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public static class ParseTreePrinter
    {

        public static string PrettyPrint(SyntaxNode node)
        {
            using(var writer = new StringWriter())
            {
                PrettyPrint(node, writer);
                return writer.ToString();
            }
        }


        public static void PrettyPrint(SyntaxNode node, TextWriter writer, string firstLinePrefix = "", string furtherLinePrefix = "")
        {
            writer.Write(firstLinePrefix);
            TryPrintWithColour(writer, ConsoleColor.DarkCyan, node.Kind);
            if(node is SyntaxToken token && token.Value != null)
            {
                writer.Write(": ");
                TryPrintWithColour(writer, ConsoleColor.DarkGreen, token.Value);
            }
            else
            {
                var children = node.GetChildren();
                var lastChild = children.LastOrDefault();
                foreach(var child in children)
                {
                    writer.WriteLine();
                    bool isLast = child == lastChild;
                    var firstPrefix = isLast ? " └─" : " ├─";
                    var furtherPrefix = isLast ? "   " : " │ ";
                    PrettyPrint(child, writer, furtherLinePrefix + firstPrefix, furtherLinePrefix + furtherPrefix);
                }
            }
        }

        private static void TryPrintWithColour(TextWriter writer, ConsoleColor colour, object text)
        {
            bool isWritingToConsole = writer == Console.Out;

            if (isWritingToConsole)
            {
                Console.ForegroundColor = colour;
                writer.Write(text);
                Console.ResetColor();
            }
            else
            {
                writer.Write(text);
            }
        }

    }
}