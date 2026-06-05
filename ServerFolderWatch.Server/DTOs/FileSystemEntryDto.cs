namespace ServerFolderWatch.Server.DTOs;

public record FileSystemEntryDto(string Name, FileSystemEntityType Type, int? Version);
