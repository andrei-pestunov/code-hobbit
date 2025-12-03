using System.ComponentModel;
using System.Globalization;
using System.Text;
using CodeHobbit.Rag;
using ModelContextProtocol.Server;

namespace CodeHobbit.McpServer.Tools;

[McpServerToolType]
internal sealed class RagTools
{
    private readonly Service _rag;

    public RagTools(Service rag)
    {
        _rag = rag;
    }

    [McpServerTool]
    [Description("Search the golden microservice for patterns and example code matching the description.")]
    public async Task<string> SearchPatternsAsync(
        [Description("Natural language description of the pattern you're looking for, e.g. 'MediatR command handler with FluentValidation and Result<T>'")]
        string query,
        int maxResults = 5)
    {
        var matches = await _rag.SearchPatternsAsync(query, maxResults);

        if (matches.Count == 0)
        {
            return "No matching golden patterns found.";
        }

        var sb = new StringBuilder();
        foreach (var m in matches)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Path: {m.Path}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Score: {m.Score:F3}");
            sb.AppendLine("Snippet:");
            sb.AppendLine(m.Snippet);
            sb.AppendLine(new string('-', 80));
        }

        return sb.ToString();
    }

    [McpServerTool]
    [Description("Find golden-service patterns similar to the provided legacy C# code.")]
    public async Task<string> SimilarToCodeAsync(
        [Description("Legacy C# code you want to modernize to golden patterns.")]
        string code,
        int maxResults = 3)
    {
        var matches = await _rag.SearchPatternsAsync(
            $"Pattern similar to this code: {code}", maxResults);

        if (matches.Count == 0)
        {
            return "No similar golden patterns found.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Here are similar patterns from the golden microservice:");
        foreach (var m in matches)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Path: {m.Path}");
            sb.AppendLine(m.Snippet);
            sb.AppendLine(new string('-', 80));
        }

        return sb.ToString();
    }
}
