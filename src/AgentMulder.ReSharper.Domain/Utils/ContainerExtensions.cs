﻿using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace AgentMulder.ReSharper.Domain.Utils
{
    public static class ContainerExtensions
    {
        public static bool IsContainerCall(this ITreeNode node, string containerClrTypeName)
        {
            var invocationExpression = node as IInvocationExpression;
            if (invocationExpression == null)
            {
                return false;
            }

            var resolve = invocationExpression.InvocationExpressionReference.Resolve().Result;
            var method = resolve.DeclaredElement as IMethod;
            if (method == null)
            {
                return false;
            }
            ITypeElement containingType = method.GetContainingType();
            if (containingType == null)
            {
                return false;
            }
            
            var containerClrType = TypeFactory.CreateTypeByCLRName(containerClrTypeName, node.GetPsiModule());

            return containingType.IsDescendantOf(containerClrType.GetTypeElement());
        }
    }
}