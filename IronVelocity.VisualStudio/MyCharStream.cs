using NVelocity.Runtime.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.VisualStudio
{
    public class MyCharStream : ICharStream
    {
        private readonly string _content;
        private TextReader _reader;
        private StringBuilder _buffer;
        public MyCharStream(string content)
        {
            _content = content;
            _reader = new StringReader(_content);
            _buffer = new StringBuilder();
        }

        public char CurrentCharacter { get; private set; }

        public int Position { get; private set; }

        public int Line { get { return 0; } }
        public int BeginLine { get { return 0; } }
        public int EndLine { get { return 0; } }

        public int Column { get { return Position; } }
        public int BeginColumn { get; private set; }
        public int EndColumn { get { return Position; } }


        public string GetImage()
        {
            return _buffer.ToString();
        }

        public char[] GetSuffix(int len)
        {
            return _buffer.ToString().Substring(_buffer.Length - len).ToCharArray();
            //return _content.Substring(Position, len).ToCharArray();
        }

        public void Backup(int amount)
        {
            var oldReader = _reader;
            var startIndex = Position - amount;
            _reader = new StringReader(_content.Substring(startIndex));
            oldReader.Dispose();
            _buffer.Remove(_buffer.Length - amount, amount);
            Position = startIndex;
        }

        public bool BeginToken()
        {
            _buffer.Clear();
            if (ReadChar())
            {
                BeginColumn = Position;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ReadChar()
        {
            var value = _reader.Read();
            if (value > 0)
            {
                Position++;
                CurrentCharacter = (char)value;
                _buffer.Append(CurrentCharacter);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Done()
        {
        }
    }
}
