using IronVelocity.Compilation;
using System;
using System.Diagnostics.Tracing;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity
{
    [EventSource(Name = "IronVelocity-TemplateGeneration")]
    internal sealed class TemplateGenerationEventSource : EventSource
    {
        private static readonly MethodInfo _debugViewMethod = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true);

        internal static readonly TemplateGenerationEventSource Log = new TemplateGenerationEventSource();

        [Event(1, Task = Tasks.Parse, Opcode = EventOpcode.Start, Level = EventLevel.Verbose)]
        public void ParseStart(string id) => WriteEvent(1, id);

        [Event(2, Task = Tasks.Parse, Opcode = EventOpcode.Stop, Level = EventLevel.Verbose)]
        public void ParseStop(string id) => WriteEvent(2, id);

        [Event(3, Task = Tasks.ConvertToExpressionTree, Opcode = EventOpcode.Start, Level = EventLevel.Verbose)]
        public void ConvertToExpressionTreeStart(string id) => WriteEvent(3, id);

        [Event(4, Task = Tasks.ConvertToExpressionTree, Opcode = EventOpcode.Stop, Level = EventLevel.Verbose)]
        public void ConvertToExpressionTreeStop(string id) => WriteEvent(4, id);

        [Event(5, Task = Tasks.CompileMethod, Opcode = EventOpcode.Start, Level = EventLevel.Verbose)]
        public void CompileMethodStart(string id) => WriteEvent(5, id);

        [Event(6, Task = Tasks.CompileMethod, Opcode = EventOpcode.Stop, Level = EventLevel.Verbose)]
        public void CompileMethodStop(string id) => WriteEvent(6, id);

        [Event(7, Task = Tasks.GenerateDebugInfo, Opcode = EventOpcode.Start, Level = EventLevel.Verbose)]
        public void GenerateDebugInfoStart(string id) => WriteEvent(7, id);

        [Event(8, Task = Tasks.GenerateDebugInfo, Opcode = EventOpcode.Stop, Level = EventLevel.Verbose)]
        public void GenerateDebugInfoStop(string id) => WriteEvent(8, id);

        [Event(9, Task = Tasks.StronglyType, Opcode = EventOpcode.Start, Level = EventLevel.Verbose)]
        public void StronglyTypeStart(string id) => WriteEvent(9, id);

        [Event(10, Task = Tasks.StronglyType, Opcode = EventOpcode.Stop, Level = EventLevel.Verbose)]
        public void StronglyTypeStop(string id) => WriteEvent(10, id);

        [Event(11, Task = Tasks.InitaliseCallSites, Opcode = EventOpcode.Start, Level = EventLevel.Verbose)]
        public void InitaliseCallSitesStart(string id) => WriteEvent(11, id);

        [Event(12, Task = Tasks.InitaliseCallSites, Opcode = EventOpcode.Stop, Level = EventLevel.Verbose)]
        public void InitaliseCallSitesStop(string id) => WriteEvent(12, id);

        [NonEvent]
        public void LogParsedExpressionTree(string id, Expression<VelocityTemplateMethod> expressionTree)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.ExpressionTree))
            {
                LogParsedExpressionTreeInternal(id, GetExpressionDebugView(expressionTree));
            }

        }

        [NonEvent]
        public void LogProcessedExpressionTree(string id, Expression<VelocityTemplateMethod> expressionTree)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.ExpressionTree))
            {
                LogProcessedExpressionTreeInternal(id, GetExpressionDebugView(expressionTree));
            }

        }

        private string GetExpressionDebugView(Expression expression)
            => (String)_debugViewMethod.Invoke(expression, new object[0]);


        [Event(13, Opcode = EventOpcode.Info, Level = EventLevel.Verbose, Keywords = Keywords.ExpressionTree)]
        private void LogParsedExpressionTreeInternal(string id, string expressionTree) => WriteEvent(13, id, expressionTree);

        [Event(14, Opcode = EventOpcode.Info, Level = EventLevel.Verbose, Keywords = Keywords.ExpressionTree)]
        private void LogProcessedExpressionTreeInternal(string id, string expressionTree) => WriteEvent(14, id, expressionTree);

        public static class Tasks
        {
            public const EventTask Parse = (EventTask)1;
            public const EventTask ConvertToExpressionTree = (EventTask)2;
            public const EventTask Optimize = (EventTask)3;
            public const EventTask CompileMethod = (EventTask)4;
            public const EventTask GenerateDebugInfo = (EventTask)5;
            public const EventTask StronglyType = (EventTask)6;
            public const EventTask InitaliseCallSites = (EventTask)7;
        }

        public static class Keywords
        {
            public const EventKeywords ExpressionTree = (EventKeywords)1;
        }
    }
}
