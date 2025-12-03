using CodeHobbit.Rag;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using OpenAI;
using Qdrant.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Configure Qdrant client
var qdrantEndpoint = builder.Configuration["QDRANT_ENDPOINT"] ?? "http://localhost:6333";
builder.Services.AddSingleton(_ => new QdrantClient(qdrantEndpoint));

// Configure vector store
builder.Services.AddQdrantVectorStore();

// Configure OpenAI embedding generator
var openAiApiKey = builder.Configuration["OPENAI_API_KEY"] ?? throw new InvalidOperationException("OPENAI_API_KEY is required");
var openAiModel = builder.Configuration["OPENAI_EMBED_MODEL"] ?? "text-embedding-3-small";
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var client = new OpenAIClient(openAiApiKey);
    return client.GetEmbeddingClient(openAiModel).AsIEmbeddingGenerator();
});

builder.Services.AddSingleton(sp =>
{
    var store = sp.GetRequiredService<VectorStore>();
    var embedder = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

    var root = builder.Configuration["RAG_REPO_PATH"]
               ?? "/rag-service"; // path in container

    return new Service(store, embedder, root);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly();

await builder.Build().RunAsync();
