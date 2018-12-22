using IronVelocity.Compilation.AST;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation
{
    public abstract class VelocityExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            var expression = node as VelocityExpression;
            if (expression != null)
                return VisitVelocityExpression(expression);
            
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitVelocityExpression(VelocityExpression node)
        {
            switch (node.VelocityExpressionType)
            {
                case VelocityExpressionType.CoerceToBoolean:
                    return VisitCoerceToBoolean((CoerceToBooleanExpression)node);
                case VelocityExpressionType.CustomDirective:
                    return VisitCustomDirective((CustomDirectiveExpression)node);
                case VelocityExpressionType.Dictionary:
                    return VisitDictionary((DictionaryExpression)node);
                case VelocityExpressionType.DictionaryString:
                    return VisitDictionaryString((DictionaryStringExpression)node);
                case VelocityExpressionType.Directive:
                    return VisitDirective((Directive)node);
                case VelocityExpressionType.DirectiveWord:
                    return VisitDirectiveWord((DirectiveWord)node);
                case VelocityExpressionType.Foreach:
                    return VisitForeach((ForeachExpression)node);
                case VelocityExpressionType.GlobalVariable:
                    return VisitGlobalVariable((GlobalVariableExpression)node);
                case VelocityExpressionType.IndexInvocation:
                    return VisitIndexInvocation((IndexInvocationExpression)node);
                case VelocityExpressionType.IntegerRange:
                    return VisitIntegerRange((IntegerRangeExpression)node);
                case VelocityExpressionType.InterpolatedString:
                    return VisitInterpolatedString((InterpolatedStringExpression)node);
                case VelocityExpressionType.Binary:
                    return VisitBinaryOperation((BinaryOperationExpression)node);
                case VelocityExpressionType.MethodInvocation:
                    return VisitMethodInvocation((MethodInvocationExpression)node);
                case VelocityExpressionType.ObjectArray:
                    return VisitObjectArray((ObjectArrayExpression)node);
                case VelocityExpressionType.PropertyAccess:
                    return VisitPropertyAccess((PropertyAccessExpression)node);
                case VelocityExpressionType.Reference:
                    return VisitReference((ReferenceExpression)node);
                case VelocityExpressionType.RenderableExpression:
                    return VisitRenderableExpression((RenderableExpression)node);
                case VelocityExpressionType.RenderedBlock:
                    return VisitRenderedBlock((RenderedBlock)node);
                case VelocityExpressionType.SetDirective:
                    return VisitSetDirective((SetDirective)node);
                case VelocityExpressionType.SetMember:
                    return VisitSetMember((SetMemberExpression)node);
                case VelocityExpressionType.SetIndex:
                    return VisitSetIndex((SetIndexExpression)node);
                case VelocityExpressionType.TemplatedForeach:
                    return VisitTemplatedForEach((TemplatedForeachExpression)node);
                case VelocityExpressionType.Variable:
                    return VisitVariable((VariableExpression)node);
                case VelocityExpressionType.TemporaryVariableScope:
                    return VisitTemporaryVariableScope((TemporaryVariableScopeExpression)node);
                default:
                    throw new InvalidOperationException($"Unexpected VelocityExpressionType: {node.VelocityExpressionType}");
            }
        }

        protected virtual Expression VisitCoerceToBoolean(CoerceToBooleanExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitCustomDirective(CustomDirectiveExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitDictionary(DictionaryExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitDictionaryString(DictionaryStringExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitDirective(Directive node) => base.VisitExtension(node);
        protected virtual Expression VisitDirectiveWord(DirectiveWord node) => base.VisitExtension(node);
        protected virtual Expression VisitForeach(ForeachExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitGlobalVariable(GlobalVariableExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitIndexInvocation(IndexInvocationExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitIntegerRange(IntegerRangeExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitInterpolatedString(InterpolatedStringExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitBinaryOperation(BinaryOperationExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitMethodInvocation(MethodInvocationExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitObjectArray(ObjectArrayExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitPropertyAccess(PropertyAccessExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitReference(ReferenceExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitRenderableExpression(RenderableExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitRenderedBlock(RenderedBlock node) => base.VisitExtension(node);
        protected virtual Expression VisitSetDirective(SetDirective node) => base.VisitExtension(node);
        protected virtual Expression VisitSetMember(SetMemberExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitSetIndex(SetIndexExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitTemplatedForEach(TemplatedForeachExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitTemporaryVariableScope(TemporaryVariableScopeExpression node) => base.VisitExtension(node);
        protected virtual Expression VisitVariable(VariableExpression node) => base.VisitExtension(node);


#if DEBUG
#pragma warning disable S1185
		//For debugging purposes, it is useful to have - easier to identify where visiting is raising errors

		protected override Expression VisitUnary(UnaryExpression node) => base.VisitUnary(node);
        protected override Expression VisitConditional(ConditionalExpression node) => base.VisitConditional(node);
        public override Expression Visit(Expression node) => base.Visit(node);
        protected override Expression VisitBinary(BinaryExpression node) => base.VisitBinary(node);
        protected override Expression VisitBlock(BlockExpression node) => base.VisitBlock(node);
        protected override CatchBlock VisitCatchBlock(CatchBlock node) => base.VisitCatchBlock(node);
        protected override Expression VisitConstant(ConstantExpression node) => base.VisitConstant(node);
        protected override Expression VisitDebugInfo(DebugInfoExpression node) => base.VisitDebugInfo(node);
        protected override Expression VisitDefault(DefaultExpression node) => base.VisitDefault(node);
        protected override ElementInit VisitElementInit(ElementInit node) => base.VisitElementInit(node);
        protected override Expression VisitGoto(GotoExpression node) => base.VisitGoto(node);
        protected override Expression VisitIndex(IndexExpression node) => base.VisitIndex(node);
        protected override Expression VisitInvocation(InvocationExpression node) => base.VisitInvocation(node);
        protected override Expression VisitLabel(LabelExpression node) => base.VisitLabel(node);
        protected override LabelTarget VisitLabelTarget(LabelTarget node) => base.VisitLabelTarget(node);
        protected override Expression VisitLambda<T>(Expression<T> node) => base.VisitLambda<T>(node);
        protected override Expression VisitListInit(ListInitExpression node) => base.VisitListInit(node);
        protected override Expression VisitLoop(LoopExpression node) => base.VisitLoop(node);
        protected override Expression VisitMember(MemberExpression node) => base.VisitMember(node);
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) => base.VisitMemberAssignment(node);
        protected override MemberBinding VisitMemberBinding(MemberBinding node) => base.VisitMemberBinding(node);
        protected override Expression VisitMemberInit(MemberInitExpression node) => base.VisitMemberInit(node);
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) => base.VisitMemberMemberBinding(node);
        protected override Expression VisitNew(NewExpression node) => base.VisitNew(node);
        protected override Expression VisitParameter(ParameterExpression node) => base.VisitParameter(node);
        protected override Expression VisitMethodCall(MethodCallExpression node) => base.VisitMethodCall(node);
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) => base.VisitRuntimeVariables(node);
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) => base.VisitMemberListBinding(node);
        protected override Expression VisitSwitch(SwitchExpression node) => base.VisitSwitch(node);
        protected override SwitchCase VisitSwitchCase(SwitchCase node) => base.VisitSwitchCase(node);
        protected override Expression VisitTry(TryExpression node) => base.VisitTry(node);
        protected override Expression VisitTypeBinary(TypeBinaryExpression node) => base.VisitTypeBinary(node);
        protected override Expression VisitDynamic(DynamicExpression node) => base.VisitDynamic(node);
        protected override Expression VisitNewArray(NewArrayExpression node) => base.VisitNewArray(node);
#pragma warning restore S1185
#endif

	}
}
