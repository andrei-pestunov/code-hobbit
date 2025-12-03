using CodeHobbit.Rag;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Configure Qdrant client
var qdrantEndpoint = builder.Configuration["QDRANT_ENDPOINT"] ?? "http://localhost:6333";

builder.Services.AddSingleton(_ => new QdrantClient(qdrantEndpoint));

// Configure vector store
builder.Services.AddQdrantVectorStore();

builder.Services.AddSingleton(sp =>
{
    var store = sp.GetRequiredService<VectorStore>();
    var embedder = sp.GetRequiredService<IEmbeddingGenerator<byte[], Embedding<float>>>();

    return new Service(store, embedder);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly();

await builder.Build().RunAsync();
