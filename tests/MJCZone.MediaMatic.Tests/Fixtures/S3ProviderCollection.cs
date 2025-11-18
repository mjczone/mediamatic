// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Tests.Fixtures;

/// <summary>
/// xUnit collection definition for S3 provider tests.
/// All test classes decorated with [Collection("S3Provider")] will share the same LocalStackFixture instance.
/// </summary>
[CollectionDefinition("S3Provider")]
public class S3ProviderCollection : ICollectionFixture<LocalStackFixture>
{
    // This class is never instantiated. It exists only to define the collection.
}
