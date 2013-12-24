using NVelocity.Runtime.Parser.Node;
using System.Collections.Generic;

namespace IronVelocity
{
    public static class INodeExtensions
    {
  
        public static IEnumerable<INode> GetChildren(this INode node)
        {
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                yield return node.GetChild(i);
            };
        }
    }


}
