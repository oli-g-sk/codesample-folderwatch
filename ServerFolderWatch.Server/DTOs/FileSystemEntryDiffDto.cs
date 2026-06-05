namespace ServerFolderWatch.Server.DTOs;

// TODO turn to class for inheritance?
public record FileSystemEntryDiffDto
(
    string Name,
    FileSystemEntityType Type,
    FileSystemEntityDiffOperation DiffOperation,
    int? Version
);