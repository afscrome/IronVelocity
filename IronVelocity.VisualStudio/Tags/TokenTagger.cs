using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NVelocity.Runtime;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IronVelocity.VisualStudio.Tags
{
    internal sealed class TokenTagger : ITagger<TokenTag>
    {
        private static readonly RuntimeInstance _runtimeServices;

        static TokenTagger()
        {
            //No idea why the followign is required = 
            var hack = Type.GetType(typeof(NVelocity.SupportClass).AssemblyQualifiedName);
            var hack3 = Type.GetType("NVelocity.Runtime.Directive.DirectiveManager,NVelocity");

            _runtimeServices = new VelocityRuntime(null)._runtimeService;
        }

        ITextBuffer _buffer;
        Parser _parser;
        internal TokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _parser = _runtimeServices.CreateNewParser();
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<TokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var tags = Enumerable.Empty<ITagSpan<TokenTag>>();
            foreach (SnapshotSpan span in spans)
            {
                var text = span.GetText();
                //Perf hack - if we don't have any of the following, then there's no special velocity
                if (text.IndexOfAny(new[] { '$', '#' }) < 0)
                    return Enumerable.Empty<ITagSpan<TokenTag>>();

                using (var reader = new StringReader(text))
                {
                    var charStream = new VelocityCharStream(reader, 0, 0, text.Length);
                    _parser.ReInit(charStream);

                    try
                    {
                        var tree = _parser.Process();
                        tags = tags.Union(GetTags(tree, span));
                    }
                    catch
                    {

                    }
                }

            }

            return tags;
        }

        private IEnumerable<ITagSpan<TokenTag>> GetTags(INode node, SnapshotSpan parent)
        {
            var currentSnapshotSpan = parent;

            var tagType = GetTokenType(node);
            if (tagType != TokenType.Ignore)
            {
                var tag = new TokenTag(tagType);
                yield return new TagSpan<TokenTag>(parent, tag);
            }

            var lastEnd = parent.Start;
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var child = node.GetChild(i);
                int length = child.Literal.Length;
                var childSpan = new SnapshotSpan(lastEnd, length);
                foreach (var tag in GetTags(child, childSpan))
                {
                    yield return tag;
                }
                lastEnd = childSpan.End;
            }
        }

        private TokenType GetTokenType(INode node)
        {
            switch (node.Type)
            {
                case ParserTreeConstants.COMMENT:
                    return TokenType.Comment;

                case ParserTreeConstants.NUMBER_LITERAL:
                    return TokenType.NumberLiteral;

                case ParserTreeConstants.STRING_LITERAL:
                    return TokenType.StringLiteral;

                case ParserTreeConstants.TRUE:
                case ParserTreeConstants.FALSE:
                case ParserTreeConstants.OBJECT_ARRAY:
                case ParserTreeConstants.INTEGER_RANGE:
                    return TokenType.Literal;

                case ParserTreeConstants.IDENTIFIER:
                    return TokenType.Identifier;

                case ParserTreeConstants.METHOD:
                case ParserTreeConstants.REFERENCE:
                    return TokenType.SymbolReference;

                case ParserTreeConstants.AND_NODE:
                case ParserTreeConstants.OR_NODE:
                case ParserTreeConstants.NOT_NODE:
                case ParserTreeConstants.LT_NODE:
                case ParserTreeConstants.LE_NODE:
                case ParserTreeConstants.GT_NODE:
                case ParserTreeConstants.GE_NODE:
                case ParserTreeConstants.EQ_NODE:
                case ParserTreeConstants.NE_NODE:
                case ParserTreeConstants.ADD_NODE:
                case ParserTreeConstants.SUBTRACT_NODE:
                case ParserTreeConstants.MUL_NODE:
                case ParserTreeConstants.DIV_NODE:
                case ParserTreeConstants.MOD_NODE:
                    return TokenType.Operator;



                case ParserTreeConstants.DIRECTIVE:
                case ParserTreeConstants.IF_STATEMENT:
                case ParserTreeConstants.SET_DIRECTIVE:
                case ParserTreeConstants.ELSE_IF_STATEMENT:
                case ParserTreeConstants.ELSE_STATEMENT:
                    return TokenType.Keyword;


                case ParserTreeConstants.ASSIGNMENT:
                case ParserTreeConstants.EXPRESSION:
                    return TokenType.Ignore;

                case ParserTreeConstants.PROCESS:
                case ParserTreeConstants.TEXT:
                case ParserTreeConstants.ESCAPE:
                case ParserTreeConstants.ESCAPED_DIRECTIVE:
                case ParserTreeConstants.VOID:
                case ParserTreeConstants.WORD:
                    return TokenType.Ignore;
                default:
                    throw new ArgumentOutOfRangeException("node");
            }
        }

    }
}
