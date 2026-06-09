using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Server.DTOs;

public record FileSystemEntryDiffDto
(
    string Name,
    FileSystemEntityType Type,
    DiffOperation DiffOperation,
    int? Version
);