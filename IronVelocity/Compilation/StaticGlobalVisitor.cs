using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using System.Dynamic;

namespace IronVelocity.Compilation
{
    public class StaticGlobalVisitor : ExpressionVisitor
    {

        private readonly IReadOnlyDictionary<string, Type> _globalTypeMap;
        public StaticGlobalVisitor(IReadOnlyDictionary<string, Type> globalTypeMap)
        {            
            _globalTypeMap = globalTypeMap;
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            var args = new Expression[node.Arguments.Count];

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                args[i] = Visit(node.Arguments[i]);
            }

            return Expression.Dynamic(node.Binder, node.Type, args);
        }

        protected override Expression VisitExtension(Expression node)
        {
  
            var variable = node as VariableExpression;
            if (variable != null)
                return VisitVariable(variable);

            var property = node as PropertyAccessExpression;
            if (property != null)
                return VisitPropertyAccess(property);

            var method = node as MethodInvocationExpression;
            if (method != null)
                return VisitMethodInvocationExpression(method);
            
            return base.VisitExtension(node);
        }

        protected virtual Expression VisitBinaryLogicalExpression(BinaryLogicalExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            if (left.Type == typeof(bool) && right.Type == typeof(bool))
            {
                if (node.Operation == LogicalOperation.And)
                    return Expression.AndAlso(left, right);
                else if (node.Operation == LogicalOperation.Or)
                    return Expression.OrElse(left, right);
            }
            return base.VisitExtension(node);
        }




        protected virtual Expression VisitVariable(VariableExpression node)
        {
            Type staticType;
            if (!_globalTypeMap.TryGetValue(node.Name, out staticType) || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(staticType))
                return base.VisitExtension(node);

            return new GlobalVariableExpression(node, staticType);
        }

        protected virtual Expression VisitPropertyAccess(PropertyAccessExpression node)
        {
            var target = Visit(node.Target);

            return node.Update(target);
        }

        protected virtual Expression VisitMethodInvocationExpression(MethodInvocationExpression node)
        {
            var target = Visit(node.Target);
            var args = new Expression[node.Arguments.Count];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Visit(node.Arguments[i]);
            }

            return node.Update(target, args);
        }





        public static bool IsConstantType(Expression expression)
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(expression.Type))
                return false;

            if (expression is ConstantExpression || expression is GlobalVariableExpression)
                return true;

            //Interpolated & dictionary strings will always return the same type
            if (expression is StringExpression || expression is InterpolatedStringExpression || expression is DictionaryStringExpression
                || expression is DictionaryExpression || expression is ObjectArrayExpression)
                return true;

            //if (expression is MethodCallExpression || expression is PropertyAccessExpression || expression is MemberExpression)
                //return true;

            if (expression.Type == typeof(void))
                return true;


            //If the type is sealed, there
            if (expression.Type.IsSealed)
                return true;

            return false;
        }







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
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return base.VisitNewArray(node);
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
    }
}
