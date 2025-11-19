// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

namespace MJCZone.MediaMatic.Tests.Fixtures;

/// <summary>
/// Collection definition for SFTP provider tests.
/// Ensures single shared SFTP testcontainer across all tests in the collection.
/// </summary>
[CollectionDefinition("SftpProvider")]
public class SftpProviderCollection : ICollectionFixture<SftpFixture> { }
