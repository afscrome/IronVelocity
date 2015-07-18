using System.Collections.Generic;

namespace IronVelocity.Parser
{
    interface ITextWindow
    {
        char CurrentChar { get; }
        char MoveNext();
        int CurrentPosition { get; }
        int StartPosition { get; }
        int EndPosition { get; }
        string GetRange(int start, int end);
    }
}
