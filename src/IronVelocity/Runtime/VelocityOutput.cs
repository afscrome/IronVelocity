using System;
using System.IO;

namespace IronVelocity.Runtime
{
    public sealed class VelocityOutput
    {
        private readonly TextWriter _writer;

        public VelocityOutput(TextWriter writer)
        {
            _writer = writer;
        }

        public void Write(string value)
        {
            _writer.Write(value);
        }

        public void Write(object value)
        {
            _writer.Write(value);
        }

        public void WriteValueType<T>(T value)
            where T : struct
        {
            Write(value.ToString());
        }

        public void Write(object value, string outputIfNull)
        {
            if (value == null)
                Write(outputIfNull);
            else
                Write(value);
        }

        public void Write(string value, string outputIfNull)
        {
            Write(value ?? outputIfNull);
        }
    }
}
