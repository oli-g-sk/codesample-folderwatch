using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Server.DTOs;

// TODO turn to class for inheritance?
public record FileSystemEntryDiffDto
(
    string Name,
    FileSystemEntityType Type,
    DiffOperation DiffOperation,
    int? Version
);