// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     The options to be used by a <see cref="DbContext"/>. You normally override 
    ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> or use a <see cref="DbContextOptionsBuilder{TContext}"/> 
    ///     to create instances of this class and it is not designed to be directly constructed in your application code. 
    /// </summary>
    /// <typeparam name="TContext"> The type of the context these options apply to. </typeparam>
    public class DbContextOptions<TContext> : DbContextOptions
        where TContext : DbContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions{TContext}" /> class. You normally override 
        ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> or use a <see cref="DbContextOptionsBuilder{TContext}"/> 
        ///     to create instances of this class and it is not designed to be directly constructed in your application code.
        /// </summary>
        public DbContextOptions()
            : base(new Dictionary<Type, IDbContextOptionsExtension>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions{TContext}" /> class. You normally override 
        ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> or use a <see cref="DbContextOptionsBuilder{TContext}"/> 
        ///     to create instances of this class and it is not designed to be directly constructed in your application code.
        /// </summary>
        /// <param name="extensions"> The extensions that store the configured options. </param>
        public DbContextOptions(
            [NotNull] IReadOnlyDictionary<Type, IDbContextOptionsExtension> extensions)
            : base(extensions)
        {
        }

        /// <summary>
        ///     Adds the given extension to the options.
        /// </summary>
        /// <typeparam name="TExtension"> The type of extension to be added. </typeparam>
        /// <param name="extension"> The extension to be added. </param>
        /// <returns> The same options instance so that multiple calls can be chained. </returns>
        public override DbContextOptions WithExtension<TExtension>(TExtension extension)
        {
            Check.NotNull(extension, nameof(extension));

            var extensions = Extensions.ToDictionary(p => p.GetType(), p => p);
            extensions[typeof(TExtension)] = extension;

            return new DbContextOptions<TContext>(extensions);
        }
    }
}
