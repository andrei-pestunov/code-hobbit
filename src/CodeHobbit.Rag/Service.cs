using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace CodeHobbit.Rag;

/// <summary>
/// Service for indexing and searching code patterns using vector embeddings.
/// </summary>
/// <param name="store">The vector store for storing embeddings.</param>
/// <param name="embedder">The embedding generator.</param>
/// <param name="rootPath">The root path to index.</param>
public sealed class Service(
    VectorStore store,
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    string rootPath)
{
    private const string COLLECTION_NAME = "golden_patterns";

    /// <summary>
    /// Indexes all .cs and .md files in the golden repository.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task IndexGoldenRepoAsync(CancellationToken ct = default)
    {
        var collection = store.GetCollection<string, Document>(COLLECTION_NAME);
        await collection.EnsureCollectionExistsAsync(ct);

        var files = Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                             .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                                         p.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

        foreach (var path in files)
        {
            var text = await File.ReadAllTextAsync(path, ct);
            var embeddingResult = await embedder.GenerateAsync(text, cancellationToken: ct);

            // Store relative path with forward slashes for cross-platform consistency
            var relativePath = Path.GetRelativePath(rootPath, path).Replace('\\', '/');

            var document = new Document
            {
                Id = Guid.NewGuid().ToString(),
                Path = relativePath,
                Text = text,
                Embedding = embeddingResult.Vector
            };

            await collection.UpsertAsync(document, ct);
        }
    }

    /// <summary>
    /// Searches for patterns matching the given query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of matching patterns.</returns>
    public async Task<IReadOnlyList<Match>> SearchPatternsAsync(
        string query,
        int maxResults = 5,
        CancellationToken ct = default)
    {
        var collection = store.GetCollection<string, Document>(COLLECTION_NAME);
        var embeddingResult = await embedder.GenerateAsync(query, cancellationToken: ct);

        var results = new List<Match>();

        await foreach (var result in collection.SearchAsync(embeddingResult.Vector, top: maxResults, cancellationToken: ct))
        {
            results.Add(new Match(
                Path: result.Record.Path,
                Score: result.Score ?? 0,
                Snippet: result.Record.Text));
        }

        return results;
    }
}
