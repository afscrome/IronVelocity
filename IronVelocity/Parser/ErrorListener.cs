using Antlr4.Runtime;
using System.Collections.Generic;


namespace IronVelocity.Parser
{
    public class ErrorListener<T> : IAntlrErrorListener<T>
    {
        private List<SyntaxErrorDetail> _errors;
        public IReadOnlyCollection<SyntaxErrorDetail> Errors { get { return _errors; } }

        public void SyntaxError(IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            if (_errors == null)
                _errors = new List<SyntaxErrorDetail>();

            _errors.Add(new SyntaxErrorDetail
            {
                Token = offendingSymbol,
                Line = line,
                Chartacter = charPositionInLine,
                Message = msg,
                Exception = e
            });
        }

        public class SyntaxErrorDetail
        {
            public T Token { get; set; }
            public int Line { get; set; }
            public int Chartacter { get; set; }
            public string Message { get; set; }
            public RecognitionException Exception { get; set; }
        }
    }
}
