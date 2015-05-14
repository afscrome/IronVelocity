using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Runtime
{
    public sealed class VelocityOutput
    {
        private readonly TextWriter _writer;

        [Obsolete("TODO: REmove")]
        public VelocityOutput()
            : this(new StringWriter())
        {

        }

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

        public void Write(VelocityOutput value)
        {
            //TODO: Implement this so we can output from nested templates to the 
            // main template without having to allocate an intermediate string
            // Once this is implemented, remove the default constructor and ToString()
            throw new NotImplementedException();
        }


        [Obsolete("TODO: REmove")]
        public override string ToString()
        {
            return _writer.ToString();
        }
    }
}
