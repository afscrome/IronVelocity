using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation
{
    /// <remarks>
    /// The String Dictionary & Interpolation ("%{key=value,key2=value}" & "hello $name" respectively)
    /// appear to be NVelocity specific.  Rather than being properly supported in the parser's grammar,
    /// they were implemented as hacks on ASTStringLiteral.
    /// 
    /// I would love to be able to redefine the grammar and hence parser to include these properly in the AST
    /// but for now I'm keeping the code as similar as possible to the NVelocity hack, only making minimal changes
    /// (i.e. returning Expressions building a dictionary, rather than an actual dictionary object)
    /// </remarks>
    public static class VelocityStrings
    {
        private static readonly MethodInfo _escapeQuoteMethodInfo = typeof(VelocityStrings).GetMethod(nameof(EscapeQuotes), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(char) }, null);
        private static readonly Expression _singleQuote = Expression.Constant('\'');
        private static readonly Expression _doubleQuote = Expression.Constant('"');


        public static string EscapeQuotes(object value, char quote)
        {
            if (value == null)
                return null;

            var type = value.GetType();
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return string.Concat(quote, value.ToString().Replace(quote.ToString(), "\\" + quote), quote);
            else
                return null;

        }

        public static Expression EscapeSingleQuote(Expression content)
        {
            return Expression.Call(
                _escapeQuoteMethodInfo,
                VelocityExpressions.ConvertIfNeeded(content, typeof(object)),
                _singleQuote
            );
        }

        public static Expression EscapeDoubleQuote(Expression content)
        {
            return Expression.Call(
                _escapeQuoteMethodInfo,
                VelocityExpressions.ConvertIfNeeded(content, typeof(object)),
                _doubleQuote
            );
        }
    }
}
