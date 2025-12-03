namespace CodeHobbit.Rag;

/// <summary>
/// Represents a matching pattern from the golden repository.
/// </summary>
/// <param name="Path">The file path of the match.</param>
/// <param name="Score">The similarity score.</param>
/// <param name="Snippet">The code snippet.</param>
public sealed record Match(string Path, double Score, string Snippet);
