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
                case VelocityExpressionType.Comparison:
                    return VisitComparison((ComparisonExpression)node);
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
                case VelocityExpressionType.IntegerRange:
                    return VisitIntegerRange((IntegerRangeExpression)node);
                case VelocityExpressionType.InterpolatedString:
                    return VisitInterpolatedString((InterpolatedStringExpression)node);
                case VelocityExpressionType.Mathematical:
                    return VisitMathematical((MathematicalExpression)node);
                case VelocityExpressionType.MethodInvocation:
                    return VisitMethodInvocation((MethodInvocationExpression)node);
                case VelocityExpressionType.ObjectArray:
                    return VisitObjectArray((ObjectArrayExpression)node);
                case VelocityExpressionType.PropertyAccess:
                    return VisitPropertyAccess((PropertyAccessExpression)node);
                case VelocityExpressionType.Reference2:
                    return VisitReference2((ReferenceExpression2)node);
                case VelocityExpressionType.Reference:
                    return VisitReference((ReferenceExpression)node);
                case VelocityExpressionType.RenderableReference:
                    return VisitRenderableReference((RenderableVelocityReference)node);
                case VelocityExpressionType.RenderableExpression:
                    return VisitRenderableExpression((RenderableExpression)node);
                case VelocityExpressionType.RenderedBlock:
                    return VisitRenderedBlock((RenderedBlock)node);
                case VelocityExpressionType.SetDirective:
                    return VisitSetDirective((SetDirective)node);
                case VelocityExpressionType.SetMember:
                    return VisitSetMember((SetMemberExpression)node);
                case VelocityExpressionType.TemplatedForeach:
                    return VisitTemplatedForEach((TemplatedForeachExpression)node);
                case VelocityExpressionType.Variable:
                    return VisitVariable((VariableExpression)node);
                case VelocityExpressionType.TemporaryVariableScope:
                    return VisitTemporaryVariableScope((TemporaryVariableScopeExpression)node);
                default:
                    throw new NotSupportedException();
            }
        }

        protected virtual Expression VisitCoerceToBoolean(CoerceToBooleanExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitComparison(ComparisonExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitCustomDirective(CustomDirectiveExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitDictionary(DictionaryExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitDictionaryString(DictionaryStringExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitDirective(Directive node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitDirectiveWord(DirectiveWord node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitForeach(ForeachExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitGlobalVariable(GlobalVariableExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitIntegerRange(IntegerRangeExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitInterpolatedString(InterpolatedStringExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitMathematical(MathematicalExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitMethodInvocation(MethodInvocationExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitObjectArray(ObjectArrayExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitPropertyAccess(PropertyAccessExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitReference(ReferenceExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitReference2(ReferenceExpression2 node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitRenderableReference(RenderableVelocityReference node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitRenderableExpression(RenderableExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitRenderedBlock(RenderedBlock node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitSetDirective(SetDirective node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitSetMember(SetMemberExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitTemplatedForEach(TemplatedForeachExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitTemporaryVariableScope(TemporaryVariableScopeExpression node)
        {
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitVariable(VariableExpression node)
        {
            return base.VisitExtension(node);
        }


#if DEBUG
        //For debugging purposes, it is useful to have - easier to identify where visiting is raising errors

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return base.VisitUnary(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return base.VisitBinary(node);
        }
        protected override Expression VisitBlock(BlockExpression node)
        {
            return base.VisitBlock(node);
        }
        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            return base.VisitCatchBlock(node);
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            return base.VisitConstant(node);
        }
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            return base.VisitDebugInfo(node);
        }
        protected override Expression VisitDefault(DefaultExpression node)
        {
            return base.VisitDefault(node);
        }
        protected override ElementInit VisitElementInit(ElementInit node)
        {
            return base.VisitElementInit(node);
        }
        protected override Expression VisitGoto(GotoExpression node)
        {
            return base.VisitGoto(node);
        }
        protected override Expression VisitIndex(IndexExpression node)
        {
            return base.VisitIndex(node);
        }
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return base.VisitInvocation(node);
        }
        protected override Expression VisitLabel(LabelExpression node)
        {
            return base.VisitLabel(node);
        }
        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            return base.VisitLabelTarget(node);
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda<T>(node);
        }
        protected override Expression VisitListInit(ListInitExpression node)
        {
            return base.VisitListInit(node);
        }
        protected override Expression VisitLoop(LoopExpression node)
        {
            return base.VisitLoop(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return base.VisitMemberAssignment(node);
        }
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return base.VisitMemberInit(node);
        }
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }
        protected override Expression VisitNew(NewExpression node)
        {
            return base.VisitNew(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            return base.VisitRuntimeVariables(node);
        }
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return base.VisitMemberListBinding(node);
        }
        protected override Expression VisitSwitch(SwitchExpression node)
        {
            return base.VisitSwitch(node);
        }
        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            return base.VisitSwitchCase(node);
        }
        protected override Expression VisitTry(TryExpression node)
        {
            return base.VisitTry(node);
        }
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return base.VisitTypeBinary(node);
        }
        protected override Expression VisitDynamic(DynamicExpression node)
        {
            return base.VisitDynamic(node);
        }
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return base.VisitNewArray(node);
        }
#endif

    }
}
