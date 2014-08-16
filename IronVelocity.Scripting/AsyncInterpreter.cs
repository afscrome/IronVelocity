/*
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Interpreter;

namespace IronVelocity.Scripting
{
    internal sealed class Interpreter
    {
        internal static readonly object NoValue = new object();
        internal const int RethrowOnReturn = 2147483647;
        internal readonly int _compilationThreshold;
        private readonly LocalVariables _locals;
        internal readonly int[] _boxedLocals;
        private readonly Dictionary<LabelTarget, BranchLabel> _labelMapping;
        private readonly InstructionArray _instructions;
        internal readonly object[] _objects;
        internal readonly RuntimeLabel[] _labels;
        internal readonly LambdaExpression _lambda;
        private readonly ExceptionHandler[] _handlers;
        internal readonly DebugInfo[] _debugInfos;
        private static ThreadAbortException _anyAbortException;

        internal bool CompileSynchronously
        {
            get
            {
                return this._compilationThreshold <= 1;
            }
        }

        internal InstructionArray Instructions
        {
            get
            {
                return this._instructions;
            }
        }

        internal LocalVariables Locals
        {
            get
            {
                return this._locals;
            }
        }

        internal Dictionary<LabelTarget, BranchLabel> LabelMapping
        {
            get
            {
                return this._labelMapping;
            }
        }

        private int ReturnAndRethrowLabelIndex
        {
            get
            {
                return this._labels.Length - 1;
            }
        }

        static Interpreter()
        {
        }

        internal Interpreter(LambdaExpression lambda, LocalVariables locals, Dictionary<LabelTarget, BranchLabel> labelMapping, InstructionArray instructions, ExceptionHandler[] handlers, DebugInfo[] debugInfos, int compilationThreshold)
        {
            this._lambda = lambda;
            this._locals = locals;
            this._boxedLocals = locals.GetBoxed();
            this._instructions = instructions;
            this._objects = instructions.Objects;
            this._labels = instructions.Labels;
            this._labelMapping = labelMapping;
            this._handlers = handlers;
            this._debugInfos = debugInfos;
            this._compilationThreshold = compilationThreshold;
        }

        [SpecialName]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Run(InterpretedFrame frame)
        {
            while (true)
            {
                try
                {
                    Instruction[] instructionArray = this._instructions.Instructions;
                    int index = frame.InstructionIndex;
                    while (index < instructionArray.Length)
                    {
                        index += instructionArray[index].Run(frame);
                        frame.InstructionIndex = index;
                    }
                    break;
                }
                catch (Exception ex)
                {
                    frame.SaveTraceToException(ex);
                    frame.FaultingInstruction = frame.InstructionIndex;
                    ExceptionHandler handler;
                    frame.InstructionIndex += this.GotoHandler(frame, (object)ex, out handler);
                    if (handler == null || handler.IsFault)
                    {
                        this.Run(frame);
                        if (frame.InstructionIndex != int.MaxValue)
                            break;
                        throw;
                    }
                    else
                    {
                        ThreadAbortException threadAbortException = ex as ThreadAbortException;
                        if (threadAbortException != null)
                        {
                            Interpreter._anyAbortException = threadAbortException;
                            frame.CurrentAbortHandler = handler;
                            this.Run(frame);
                            break;
                        }
                    }
                }
            }
        }

        internal static void AbortThreadIfRequested(InterpretedFrame frame, int targetLabelIndex)
        {
            ExceptionHandler exceptionHandler = frame.CurrentAbortHandler;
            if (exceptionHandler == null || exceptionHandler.IsInside(frame.Interpreter._labels[targetLabelIndex].Index))
                return;
            frame.CurrentAbortHandler = (ExceptionHandler)null;
            Thread currentThread = Thread.CurrentThread;
            if ((currentThread.ThreadState & ThreadState.AbortRequested) == ThreadState.Running)
                return;
            currentThread.Abort(Interpreter._anyAbortException.ExceptionState);
        }

        internal ExceptionHandler GetBestHandler(int instructionIndex, Type exceptionType)
        {
            ExceptionHandler other = (ExceptionHandler)null;
            foreach (ExceptionHandler exceptionHandler in this._handlers)
            {
                if (exceptionHandler.Matches(exceptionType, instructionIndex) && exceptionHandler.IsBetterThan(other))
                    other = exceptionHandler;
            }
            return other;
        }

        internal int GotoHandler(InterpretedFrame frame, object exception, out ExceptionHandler handler)
        {
            handler = this.GetBestHandler(frame.InstructionIndex, exception.GetType());
            if (handler == null)
                return frame.Goto(this.ReturnAndRethrowLabelIndex, Interpreter.NoValue);
            else
                return frame.Goto(handler.LabelIndex, exception);
        }
    }
}
*/