using Microsoft.VisualStudio.Text.Tagging;

namespace IronVelocity.VisualStudio.Tags
{
    public class TokenTag : ITag
    {
        public TokenType Type {get; private set;}

        public TokenTag(TokenType type)
        {
            Type = type;
        }
    }

}
