using System;

namespace IronVelocity.Parser
{
    public class StringTextWindow : ITextWindow
    {
        private readonly string _text;

        public int CurrentPosition { get; private set; }
        public char CurrentChar { get; private set; }
        public int EndPosition { get { return _text.Length; } }
        public int StartPosition { get { return 0; } }

        public StringTextWindow(string text)
        {
            _text = text;
            CurrentPosition = -1;
            MoveNext();
        }

        public char MoveNext()
        {
            var nextPosition = CurrentPosition+1;
            if (nextPosition >= _text.Length)
            {
                if (nextPosition == _text.Length)
                {
                    CurrentPosition++;
                    return CurrentChar = '\0';
                }
                else
                    throw new InvalidOperationException("Can't advance past end of string");
            }

            CurrentPosition++;
            return CurrentChar = _text[nextPosition];
        }

        public string GetRange(int start, int end)
        {
            return _text.Substring(start, end - start + 1);
        }
    }
}
