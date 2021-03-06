// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.Data.Entity.Query.ResultOperators.Internal
{
    public class FromSqlExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            RelationalQueryableExtensions.FromSqlMethodInfo
        };

        private readonly Expression _sql;
        private readonly string _argumentsParameterName;

        public FromSqlExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] Expression sql,
            [NotNull] ParameterExpression arguments)
            : base(parseInfo, null, null)
        {
            _sql = sql;
            _argumentsParameterName = arguments.Name;
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
            => new FromSqlResultOperator(_sql, _argumentsParameterName);

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
            => Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
    }
}
