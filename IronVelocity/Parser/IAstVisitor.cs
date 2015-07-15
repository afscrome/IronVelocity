using IronVelocity.Parser.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser
{
    public interface IAstVisitor<T>
    {
        T Visit(SyntaxNode node);
        T VisitArgumentsNode(ArgumentsNode node);
        T VisitBinaryExpressionNode(BinaryExpressionNode node);
        T VisitBooleanLiteralNode(BooleanLiteralNode node);
        T VisitFloatingPointLiteralNode(FloatingPointLiteralNode node);
        T VisitIntegerLiteralNode(IntegerLiteralNode node);
        T VisitListExpressionNode(ListExpressionNode node);
        T VisitMethod(Method node);
        T VisitProperty(Property node);
        T VisitReferenceNode(ReferenceNode node);
        T VisitRenderedOutputNode(RenderedOutputNode node);
        T VisitStringNode(StringNode node);
        T VisitTextNode(TextNode node);
        T VisitUnaryExpression(UnaryExpressionNode node);
        T VisitVariable(Variable node);
        T VisitWordNode(WordNode node);
    }
}
