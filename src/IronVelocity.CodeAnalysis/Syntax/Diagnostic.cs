using IronVelocity.CodeAnalysis.Text;
using System.Diagnostics;

namespace IronVelocity.CodeAnalysis.Syntax
{
    [DebuggerDisplay("{Message}")]
    public class Diagnostic
    {
        public Diagnostic(TextSpan span, string message)
        {
            Span = span;
            Message = message;
        }

        public TextSpan Span { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
