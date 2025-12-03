using Microsoft.Extensions.VectorData;

namespace CodeHobbit.Rag;

/// <summary>
/// Record representing a document in the vector store.
/// </summary>
public sealed class Document
{
    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    [VectorStoreKey]
    public Guid Id { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets or sets the file path of the document.
    /// </summary>
    [VectorStoreData]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content of the document.
    /// </summary>
    [VectorStoreData]
    public byte[] Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the embedding vector for the document.
    /// </summary>
    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
