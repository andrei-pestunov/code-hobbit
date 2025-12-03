using System.ComponentModel;
using System.Globalization;
using System.Text;
using ModelContextProtocol.Server;

namespace CodeHobbit.McpServer.Tools;

[McpServerToolType]
internal sealed class CodeTools(Rag.Service rag)
{
    private const string COLLECTION_NAME = "golden_patterns";

    [McpServerTool]
    [Description("Search the golden microservice for patterns and example code matching the description.")]
    public async Task<string> SearchPatterns(
        [Description("Natural language description of the pattern you're looking for, e.g. 'MediatR command handler with FluentValidation and Result<T>'")]
        string query)
    {
        var sb = new StringBuilder();

        await foreach (var match in rag.Search(COLLECTION_NAME, Encoding.UTF8.GetBytes(query)))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Path: {match.Path}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Score: {match.Score:F3}");
            sb.AppendLine("Snippet:");
            sb.AppendLine(Encoding.UTF8.GetString(match.Data));
            sb.AppendLine(new string('-', 80));
        }

        return sb.Length == 0 ? "No matching golden patterns found." : sb.ToString();
    }

    [McpServerTool]
    [Description("Find golden-service patterns similar to the provided legacy C# code.")]
    public async Task<string> SimilarToCode(
        [Description("Legacy C# code you want to modernize to golden patterns.")]
        string code)
    {
        var query = Encoding.UTF8.GetBytes($"Pattern similar to this code: {code}");

        var sb = new StringBuilder();
        sb.AppendLine("Here are similar patterns from the golden microservice:");

        await foreach (var match in rag.Search(COLLECTION_NAME, query, maxResults: 5))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Path: {match.Path}");
            sb.AppendLine(Encoding.UTF8.GetString(match.Data));
            sb.AppendLine(new string('-', 80));
        }

        return sb.Length <= 60 ? "No similar golden patterns found." : sb.ToString();
    }
}
