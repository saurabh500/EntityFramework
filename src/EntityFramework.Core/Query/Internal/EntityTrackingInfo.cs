// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class EntityTrackingInfo
    {
        private static readonly IEnumerable<IncludedEntity> _emptyIncludedEntities
            = Enumerable.Empty<IncludedEntity>();

        private readonly IEntityType _entityType;
        private readonly KeyValueFactory _keyValueFactory;
        private readonly IReadOnlyList<IReadOnlyList<INavigation>> _includedNavigationPaths;
        private readonly IDictionary<INavigation, IncludedEntityTrackingInfo> _includedEntityTrackingInfos;

        public EntityTrackingInfo(
            [NotNull] IKeyValueFactorySource keyValueFactorySource,
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] QuerySourceReferenceExpression querySourceReferenceExpression,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(keyValueFactorySource, nameof(keyValueFactorySource));
            Check.NotNull(querySourceReferenceExpression, nameof(querySourceReferenceExpression));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            QuerySourceReferenceExpression = querySourceReferenceExpression;

            _entityType = entityType;

            _keyValueFactory = keyValueFactorySource.GetKeyFactory(_entityType.FindPrimaryKey());

            _includedNavigationPaths
                = queryCompilationContext
                    .GetTrackableIncludes(querySourceReferenceExpression.ReferencedQuerySource);

            if (_includedNavigationPaths != null)
            {
                _includedEntityTrackingInfos = new Dictionary<INavigation, IncludedEntityTrackingInfo>();

                foreach (var navigation
                    in _includedNavigationPaths.SelectMany(ns => ns))
                {
                    if (!_includedEntityTrackingInfos.ContainsKey(navigation))
                    {
                        var targetEntityType = navigation.GetTargetType();

                        _includedEntityTrackingInfos.Add(
                            navigation,
                            new IncludedEntityTrackingInfo(
                                targetEntityType,
                                keyValueFactorySource.GetKeyFactory(targetEntityType.FindPrimaryKey())));
                    }
                }
            }
        }

        public virtual QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }
        public virtual IQuerySource QuerySource => QuerySourceReferenceExpression.ReferencedQuerySource;

        public virtual void StartTracking(
            [NotNull] IStateManager stateManager, [NotNull] object entity, ValueBuffer valueBuffer)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(entity, nameof(entity));

            stateManager.StartTracking(
                _entityType,
                _keyValueFactory.Create(valueBuffer),
                entity,
                valueBuffer);
        }

        public class IncludedEntityTrackingInfo
        {
            public IncludedEntityTrackingInfo(
                [NotNull] IEntityType entityType, [NotNull] KeyValueFactory keyValueFactory)
            {
                Check.NotNull(entityType, nameof(entityType));
                Check.NotNull(keyValueFactory, nameof(keyValueFactory));

                EntityType = entityType;
                KeyValueFactory = keyValueFactory;
            }

            public virtual IEntityType EntityType { get; }

            private KeyValueFactory KeyValueFactory { get; }

            public virtual IKeyValue CreateKeyValue(ValueBuffer valueBuffer)
                => KeyValueFactory.Create(valueBuffer);
        }

        public struct IncludedEntity
        {
            public IncludedEntity(
                [NotNull] object entity, [NotNull] IncludedEntityTrackingInfo includedEntityTrackingInfo)
            {
                Check.NotNull(entity, nameof(entity));
                Check.NotNull(includedEntityTrackingInfo, nameof(includedEntityTrackingInfo));

                IncludedEntityTrackingInfo = includedEntityTrackingInfo;
                Entity = entity;
            }

            public object Entity { get; }

            private IncludedEntityTrackingInfo IncludedEntityTrackingInfo { get; }

            public void StartTracking([NotNull] IStateManager stateManager, ValueBuffer valueBuffer)
            {
                Check.NotNull(stateManager, nameof(stateManager));

                stateManager.StartTracking(
                    IncludedEntityTrackingInfo.EntityType,
                    IncludedEntityTrackingInfo.CreateKeyValue(valueBuffer),
                    Entity,
                    valueBuffer);
            }
        }

        public virtual IEnumerable<IncludedEntity> GetIncludedEntities([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            if (_includedNavigationPaths == null)
            {
                return _emptyIncludedEntities;
            }

            return _includedNavigationPaths
                .SelectMany(navigations => GetIncludedEntities(entity, navigations, index: 0));
        }

        private IEnumerable<IncludedEntity> GetIncludedEntities(
            object entity, IReadOnlyList<INavigation> navigationPath, int index)
        {
            if (index < navigationPath.Count)
            {
                var navigation = navigationPath[index];

                if (navigation.IsCollection())
                {
                    var propertyGetter = navigation.GetGetter();
                    var referencedEntities = (IEnumerable<object>)propertyGetter.GetClrValue(entity);

                    foreach (var referencedEntity
                        in referencedEntities.Where(referencedEntity => referencedEntity != null))
                    {
                        yield return new IncludedEntity(referencedEntity, _includedEntityTrackingInfos[navigation]);

                        foreach (var includedEntity
                            in GetIncludedEntities(referencedEntity, navigationPath, index + 1))
                        {
                            yield return includedEntity;
                        }
                    }
                }
                else
                {
                    var referencedEntity = navigation.GetGetter().GetClrValue(entity);

                    if (referencedEntity != null)
                    {
                        yield return new IncludedEntity(referencedEntity, _includedEntityTrackingInfos[navigation]);

                        foreach (var includedEntity
                            in GetIncludedEntities(referencedEntity, navigationPath, index + 1))
                        {
                            yield return includedEntity;
                        }
                    }
                }
            }
        }
    }
}
