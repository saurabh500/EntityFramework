// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.ValueGeneration
{
    /// <summary>
    ///     Generates <see cref="Guid"/> values using <see cref="Guid.NewGuid()"/>. 
    ///     The generated values are non-temporary, meaning they will be saved to the database.
    /// </summary>
    public class GuidValueGenerator : ValueGenerator<Guid>
    {
        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <returns> The value to be assigned to a property. </returns>
        public override Guid Next() => Guid.NewGuid();

        /// <summary>
        ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
        ///     always returns false, meaning the generated values will be saved to the database.
        /// </summary>
        public override bool GeneratesTemporaryValues => false;
    }
}
