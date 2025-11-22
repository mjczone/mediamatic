namespace MJCZone.MediaMatic.AspNetCore;

/// <summary>
/// Utility class for identifying MediaMatic operation tags.
/// </summary>
public static class OperationTags
{
    /// <summary>
    /// Tag for all MediaMatic Datasource-related operations.
    /// </summary>
    public const string Filesources = "MediaMatic Filesources";

    /// <summary>
    /// Tag for all MediaMatic Folder-related operations.
    /// </summary>
    public const string FilesourceFolders = "MediaMatic Folders";

    /// <summary>
    /// Tag for all MediaMatic File-related operations.
    /// </summary>
    public const string FilesourceFiles = "MediaMatic Files";

    /// <summary>
    /// Tag for all MediaMatic Utility-related operations.
    /// </summary>
    public const string FilesourceUtilities = "MediaMatic Utilities";
}
