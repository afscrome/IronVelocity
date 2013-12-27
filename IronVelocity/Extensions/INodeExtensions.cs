using NVelocity.Runtime.Parser.Node;
using System.Collections.Generic;

namespace IronVelocity
{
    internal static class INodeExtensions
    {  
        internal static IEnumerable<INode> GetChildren(this INode node)
        {
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                yield return node.GetChild(i);
            };
        }
    }


}
