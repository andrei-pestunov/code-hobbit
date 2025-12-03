using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace CodeHobbit.Rag;

/// <summary>
/// Service for indexing and searching code patterns using vector embeddings.
/// </summary>
/// <param name="store">The vector store for storing embeddings.</param>
/// <param name="embedder">The embedding generator.</param>
public sealed class Service(VectorStore store, IEmbeddingGenerator<byte[], Embedding<float>> embedder)
{
    private static readonly EmbeddingGenerationOptions Options = new ()
    {
        Dimensions = 1536
    };

    /// <summary>
    /// Indexes files in the specified root path with given extensions.
    /// </summary>
    /// <param name="rootPath">The root path to index.</param>
    /// <param name="extensions">File extensions to include in the index.</param>
    /// <param name="collectionName">The name of the collection to store the indexed data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Index(string rootPath, ISet<string> extensions, string collectionName, CancellationToken ct = default)
    {
        var collection = store.GetCollection<string, Document>(collectionName);

        await collection.EnsureCollectionExistsAsync(ct);

        var files = Directory
            .EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(file => extensions.Contains(Path.GetExtension(file)))
            .ToList();

        const int BATCH_SIZE = 50;

        foreach (var batch in files.Chunk(BATCH_SIZE))
        {
            ct.ThrowIfCancellationRequested();

            var documents = new List<Document>(batch.Length);

            foreach (var path in batch)
            {
                var data = await File.ReadAllBytesAsync(path, ct);

                if (data.Length == 0)
                {
                    continue;
                }

                var embedding = await embedder.GenerateAsync(data, Options, ct);
                var relativePath = Path.GetRelativePath(rootPath, path);

                documents.Add(new ()
                {
                    Id = Guid.CreateVersion7(),
                    Path = relativePath,
                    Data = data,
                    Embedding = embedding.Vector
                });
            }

            if (documents.Count > 0)
            {
                await collection.UpsertAsync(documents, ct);
            }
        }
    }

    /// <summary>
    /// Searches for patterns matching the given query.
    /// </summary>
    /// <param name="collectionName">The name of the collection to search.</param>
    /// <param name="query">The search query.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of matching patterns.</returns>
    public async IAsyncEnumerable<Match> Search(string collectionName, byte[] query, int maxResults = 5, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var collection = store.GetCollection<string, Document>(collectionName);
        var embedding = await embedder.GenerateAsync(query, Options, ct);

        await foreach (var result in collection.SearchAsync(embedding.Vector, top: maxResults, cancellationToken: ct))
        {
            yield return new Match(
                Path: result.Record.Path,
                Score: result.Score ?? 0,
                Data: result.Record.Data);
        }
    }
}
