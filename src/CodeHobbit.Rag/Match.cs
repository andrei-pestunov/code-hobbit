namespace CodeHobbit.Rag;

/// <summary>
/// Represents a match result from a search.
/// </summary>
/// <param name="Path">The file path of the match.</param>
/// <param name="Score">The similarity score.</param>
/// <param name="Data">The matched data snippet.</param>
public sealed record Match(string Path, double Score, byte[] Data);
