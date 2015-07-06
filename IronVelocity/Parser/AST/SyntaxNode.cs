
namespace IronVelocity.Parser.AST
{
    public abstract class SyntaxNode
    {
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }


}
