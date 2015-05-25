using System.Diagnostics;

namespace IronVelocity.Parser
{
    [DebuggerDisplay("Token: {TokenKind}")]
    public class Token
    {
        public TokenKind TokenKind;
        public string Value;
    }
}
