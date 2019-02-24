
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
            writer.Write(node.Kind);
            if(node is SyntaxToken token && token.Value != null)
            {
                writer.Write(": ");
                writer.Write(token.Value);
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

    }
}