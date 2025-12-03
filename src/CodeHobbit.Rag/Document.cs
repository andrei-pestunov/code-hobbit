using Microsoft.Extensions.VectorData;

namespace CodeHobbit.Rag;

/// <summary>
/// Record representing a code document in the vector store.
/// </summary>
public sealed class Document
{
    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    [VectorStoreKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path of the document.
    /// </summary>
    [VectorStoreData]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text content of the document.
    /// </summary>
    [VectorStoreData]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the embedding vector for the document.
    /// </summary>
    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
