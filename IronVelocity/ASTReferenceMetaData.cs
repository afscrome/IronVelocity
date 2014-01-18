using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This is mostly copy n' paste from the original NVelocity ASTReference class in order to expose data from private methods
    /// </remarks>
    public class ASTReferenceMetaData
    {
        private ReferenceType referenceType;

        private String nullString = string.Empty,
            escPrefix = String.Empty,
            morePrefix = String.Empty;

        private bool computableReference = true,
            escaped = false;

        public string RootString { get; private set; }
        public ReferenceType Type { get { return referenceType; } }
        public bool Escaped { get { return escaped; } }
        public string EscapePrefix { get { return escPrefix; } }
        public string NullString { get { return nullString; } }
        public string MoreString { get { return morePrefix; } }
        private bool ComputableReference { get { return computableReference; } }

        public ASTReferenceMetaData(ASTReference node)
        {
            RootString = GetRootString(node);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.StartsWith(System.String)")]
        private string GetRootString(ASTReference node)
        {
            Token t = node.FirstToken;

            // we have a special case where something like 
            // $(\\)*!, where the user want's to see something
            // like $!blargh in the output, but the ! prevents it from showing.
            // I think that at this point, this isn't a reference.

            // so, see if we have "\\!"

            int slashBang = t.Image.IndexOf("\\!");

            if (slashBang != -1)
            {
                // lets do all the work here.  I would argue that if this occurs, it's 
                // not a reference at all, so preceeding \ characters in front of the $
                // are just schmoo.  So we just do the escape processing trick (even | odd)
                // and move on.  This kind of breaks the rule pattern of $ and # but '!' really
                // tosses a wrench into things.

                // count the escapes : even # -> not escaped, odd -> escaped
                int i = 0;
                int len = t.Image.Length;

                i = t.Image.IndexOf('$');

                if (i == -1)
                {
                    // yikes!
                    //runtimeServices.Error("ASTReference.getRoot() : internal error : no $ found for slashbang.");
                    computableReference = false;
                    nullString = t.Image;
                    return nullString;
                }

                while (i < len && t.Image[i] != '\\')
                {
                    i++;
                }

                // ok, i is the first \ char
                int start = i;
                int count = 0;

                while (i < len && t.Image[i++] == '\\')
                {
                    count++;
                }

                // now construct the output string.  We really don't care about leading 
                // slashes as this is not a reference.  It's quasi-schmoo
                nullString = t.Image.Substring(0, (start) - (0)); // prefix up to the first
                nullString += t.Image.Substring(start, (start + count - 1) - (start)); // get the slashes
                nullString += t.Image.Substring(start + count); // and the rest, including the

                // this isn't a valid reference, so lets short circuit the value and set calcs
                computableReference = false;

                return nullString;
            }

            // we need to see if this reference is escaped.  if so
            // we will clean off the leading \'s and let the 
            // regular behavior determine if we should output this
            // as \$foo or $foo later on in render(). Laziness..
            escaped = false;

            if (t.Image.StartsWith(@"\"))
            {
                // count the escapes : even # -> not escaped, odd -> escaped
                int i = 0;
                int len = t.Image.Length;

                while (i < len && t.Image[i] == '\\')
                {
                    i++;
                }

                if ((i % 2) != 0)
                {
                    escaped = true;
                }

                if (i > 0)
                {
                    escPrefix = t.Image.Substring(0, (i / 2) - (0));
                }

                t.Image = t.Image.Substring(i);
            }

            // Look for preceeding stuff like '#' and '$'
            // and snip it off, except for the
            // last $
            int loc1 = t.Image.LastIndexOf('$');

            // if we have extra stuff, loc > 0
            // ex. '#$foo' so attach that to 
            // the prefix.
            if (loc1 > 0)
            {
                morePrefix = morePrefix + t.Image.Substring(0, (loc1) - (0));
                t.Image = t.Image.Substring(loc1);
            }

            // Now it should be clean. Get the literal in case this reference 
            // isn't backed by the context at runtime, and then figure out what
            // we are working with.

            nullString = node.Literal;

            if (t.Image.StartsWith("$!"))
            {
                referenceType = ReferenceType.Quiet;

                // only if we aren't escaped do we want to null the output
                if (!escaped)
                {
                    nullString = string.Empty;
                }

                if (t.Image.StartsWith("$!{"))
                {
                    // ex : $!{provider.Title} 
                    return t.Next.Image;
                }
                else
                {
                    // ex : $!provider.Title
                    return t.Image.Substring(2);
                }
            }
            else if (t.Image.Equals("${"))
            {
                // ex : ${provider.Title}
                referenceType = ReferenceType.Formal;
                return t.Next.Image;
            }
            else if (t.Image.StartsWith("$"))
            {
                // just nip off the '$' so we have 
                // the root
                referenceType = ReferenceType.Normal;
                return t.Image.Substring(1);
            }
            else
            {
                // this is a 'RUNT', which can happen in certain circumstances where
                // the parser is fooled into believing that an IDENTIFIER is a real 
                // reference.  Another 'dreaded' MORE hack :). 
                referenceType = ReferenceType.Runt;
                return t.Image;
            }
        }

        public enum ReferenceType
        {
            Normal = 1,
            Formal = 2,
            Quiet = 3,
            Runt = 4,
        }
    }
}
