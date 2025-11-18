// Copyright 2025 MJCZone Inc.
// SPDX-License-Identifier: LGPL-3.0-or-later
// Licensed under the GNU Lesser General Public License v3.0 or later.
// See LICENSE in the project root for license information.

using Amazon.S3.Model;
using MJCZone.MediaMatic.Interfaces;
using MJCZone.MediaMatic.Providers.Base;

namespace MJCZone.MediaMatic.Providers.S3;

/// <summary>
/// Represents the S3 specific VFS methods.
/// </summary>
public partial class S3Methods : VfsMethodsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="S3Methods"/> class.
    /// </summary>
    internal S3Methods()
        : base(VfsProviderType.S3) { }

    /// <inheritdoc/>
    public override bool SupportsBuckets => true;

    /// <inheritdoc/>
    public override bool SupportsNativeEncryptedStorage => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    /// <remarks>
    /// This method uses AWS SDK directly instead of FluentStorage due to a bug in FluentStorage 6.0.0+
    /// where ListAsync throws ArgumentNullException when S3 returns null object collections.
    /// See: https://github.com/robinrodricks/FluentStorage/issues/120
    /// </remarks>
    public override async Task<IEnumerable<string>> ListFilesAsync(
        IVfsConnection vfs,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        if (vfs is not S3VfsConnection s3Connection)
        {
            throw new InvalidOperationException("Invalid VFS connection type for S3 provider");
        }

        // Normalize the path prefix for S3
        var prefix = string.IsNullOrWhiteSpace(path) ? string.Empty : path.TrimEnd('/') + "/";

        var fileKeys = new List<string>();

        // List all objects with the specified prefix using AWS SDK directly
        var request = new ListObjectsV2Request
        {
            BucketName = s3Connection.BucketName,
            Prefix = prefix,
        };

        ListObjectsV2Response response;
        do
        {
            response = await s3Connection
                .S3Client.ListObjectsV2Async(request, cancellationToken)
                .ConfigureAwait(false);

            // Add all objects that are NOT folders (don't end with /)
            foreach (var s3Object in response.S3Objects)
            {
                if (!s3Object.Key.EndsWith('/'))
                {
                    fileKeys.Add(s3Object.Key);
                }
            }

            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);

        return fileKeys;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method uses AWS SDK directly instead of FluentStorage due to a bug in FluentStorage 6.0.0+
    /// where ListAsync throws ArgumentNullException when S3 returns null object collections.
    /// See: https://github.com/robinrodricks/FluentStorage/issues/120
    /// </remarks>
    public override async Task<IEnumerable<string>> ListFoldersAsync(
        IVfsConnection vfs,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        if (vfs is not S3VfsConnection s3Connection)
        {
            throw new InvalidOperationException("Invalid VFS connection type for S3 provider");
        }

        // Normalize the path prefix for S3
        var prefix = string.IsNullOrWhiteSpace(path) ? string.Empty : path.TrimEnd('/') + "/";

        var folderKeys = new HashSet<string>();

        // List all objects with the specified prefix using AWS SDK directly
        var request = new ListObjectsV2Request
        {
            BucketName = s3Connection.BucketName,
            Prefix = prefix,
            Delimiter = "/", // Use delimiter to get folder-like behavior
        };

        ListObjectsV2Response response;
        do
        {
            response = await s3Connection
                .S3Client.ListObjectsV2Async(request, cancellationToken)
                .ConfigureAwait(false);

            // CommonPrefixes contains the "folders" (prefixes that end with the delimiter)
            if (response.CommonPrefixes != null)
            {
                foreach (var commonPrefix in response.CommonPrefixes)
                {
                    // Remove trailing slash to match FluentStorage behavior
                    folderKeys.Add(commonPrefix.TrimEnd('/'));
                }
            }

            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);

        return folderKeys;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method uses AWS SDK directly instead of FluentStorage due to a bug in FluentStorage 6.0.0+
    /// where ListAsync throws ArgumentNullException when S3 returns null object collections.
    /// See: https://github.com/robinrodricks/FluentStorage/issues/120
    /// </remarks>
    public override async Task DeleteFolderAsync(
        IVfsConnection vfs,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        if (vfs is not S3VfsConnection s3Connection)
        {
            throw new InvalidOperationException("Invalid VFS connection type for S3 provider");
        }

        // S3 uses forward slashes as path separators - ensure proper prefix format
        var prefix = path.TrimEnd('/') + "/";

        var objectsToDelete = new List<KeyVersion>();

        // List all objects with the specified prefix using AWS SDK directly
        var request = new ListObjectsV2Request
        {
            BucketName = s3Connection.BucketName,
            Prefix = prefix,
        };

        ListObjectsV2Response response;
        do
        {
            response = await s3Connection
                .S3Client.ListObjectsV2Async(request, cancellationToken)
                .ConfigureAwait(false);

            // Add all objects to the delete list
            foreach (var s3Object in response.S3Objects)
            {
                objectsToDelete.Add(new KeyVersion { Key = s3Object.Key });
            }

            // Set continuation token for pagination
            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);

        // Delete all objects in batches (S3 allows max 1000 per request)
        if (objectsToDelete.Count > 0)
        {
            const int batchSize = 1000;
            for (int i = 0; i < objectsToDelete.Count; i += batchSize)
            {
                var batch = objectsToDelete.Skip(i).Take(batchSize).ToList();
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = s3Connection.BucketName,
                    Objects = batch,
                };

                await s3Connection
                    .S3Client.DeleteObjectsAsync(deleteRequest, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
