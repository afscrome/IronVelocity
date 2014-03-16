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
        ITextBuffer _buffer;
        ParserTokenManager _parser;
        internal TokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _parser = new ParserTokenManager(null);
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
                    yield break;

                var charStream = new MyCharStream(text);// new VelocityCharStream(reader, 0, 0, text.Length);
                _parser.ReInit(charStream);

                int position = span.Start.Position;

                while (true)
                {
                    Token token;
                    try
                    {
                        token = _parser.NextToken;
                    }
                    catch
                    {
                        //TODO: return some kind of error
                        yield break;
                    }
                    if (token.Kind == ParserConstants.EOF)
                        yield break;

                    var tag = TagToken(token, position, span.Snapshot);
                    if (tag != null)
                        yield return tag;

                    position += token.Image.Length;
                }

            }
        }

        private ITagSpan<TokenTag> TagToken(Token token, int position, ITextSnapshot snapshot)
        {
            var type = GetTokenType(token);
            if (type == TokenType.Ignore)
                return null;

            var span = new SnapshotSpan(snapshot, position, token.Image.Length);
            var tag = new TokenTag(type);
            return new TagSpan<TokenTag>(span, tag);
        }


        private TokenType GetTokenType(Token token)
        {
            switch (token.Kind)
            {
                case ParserConstants.LOGICAL_AND:
                case ParserConstants.LOGICAL_EQUALS:
                case ParserConstants.LOGICAL_GE:
                case ParserConstants.LOGICAL_GT:
                case ParserConstants.LOGICAL_LE:
                case ParserConstants.LOGICAL_LT:
                case ParserConstants.LOGICAL_NOT:
                case ParserConstants.LOGICAL_NOT_EQUALS:
                case ParserConstants.LOGICAL_OR:
                case ParserConstants.EQUALS:
                case ParserConstants.PLUS:
                case ParserConstants.MINUS:
                case ParserConstants.MULTIPLY:
                case ParserConstants.DIVIDE:
                case ParserConstants.MODULUS:
                case ParserConstants.DOUBLEDOT:
                    return TokenType.Operator;


                case ParserConstants.IF_DIRECTIVE:
                case ParserConstants.ELSE_DIRECTIVE:
                case ParserConstants.ELSEIF_DIRECTIVE:
                case ParserConstants.END:
                case ParserConstants.SET_DIRECTIVE:
                case ParserConstants.STOP_DIRECTIVE:
                case ParserConstants.WORD: //Directives are words - e.g. #RegisterEndOfPageHtml
                    return TokenType.Keyword;

                case ParserConstants.NUMBER_LITERAL:
                    return TokenType.NumberLiteral;

                case ParserConstants.TRUE:
                case ParserConstants.FALSE:
                    return TokenType.BooleanLiteral;

                case ParserConstants.FORMAL_COMMENT:
                case ParserConstants.MULTI_LINE_COMMENT:
                case ParserConstants.SINGLE_LINE_COMMENT:
                    return TokenType.Comment;

                case ParserConstants.STRING_LITERAL:
                    return TokenType.Comment;

                case ParserConstants.IDENTIFIER:
                    return TokenType.Identifier;

                case ParserConstants.DOT:
                case ParserConstants.COMMA:
                    return TokenType.Punctuator;


                case ParserConstants.LCURLY:
                    return TokenType.FormalReferenceStart;
                case ParserConstants.RCURLY:
                    return TokenType.FormalReferenceEnd;

                //TODO: Use for Outlining
                case ParserConstants.LBRACKET:
                case ParserConstants.LPAREN:
                case ParserConstants.RPAREN:
                case ParserConstants.RBRACKET:
                    return TokenType.Ignore;

                case ParserConstants.ALPHA_CHAR:
                case ParserConstants.ALPHANUM_CHAR:
                case ParserConstants.DIRECTIVE_CHAR:
                case ParserConstants.DIRECTIVE_TERMINATOR:
                case ParserConstants.DOLLAR:
                case ParserConstants.DOLLARBANG:
                case ParserConstants.DOUBLE_ESCAPE:
                case ParserConstants.ESCAPE:
                case ParserConstants.ESCAPE_DIRECTIVE:
                case ParserConstants.HASH:
                case ParserConstants.IDENTIFIER_CHAR:
                case ParserConstants.LETTER:
                case ParserConstants.DIGIT:
                case ParserConstants.REFERENCE_TERMINATOR:
                case ParserConstants.REFMOD2_RPAREN:
                case ParserConstants.TEXT:
                case ParserConstants.WHITESPACE:
                case ParserConstants.NEWLINE:
                case ParserConstants.EOF:
                default:
                    return TokenType.Ignore;
                    throw new NotImplementedException();
            }
        }


    }
}
