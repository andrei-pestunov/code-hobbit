using System.ComponentModel;
using System.Globalization;
using System.Text;
using ModelContextProtocol.Server;

#pragma warning disable S3400

namespace CodeHobbit.McpServer.Tools;

[McpServerToolType]
internal sealed class CodeTools(Rag.Service rag)
{
    private const string COLLECTION_NAME = "golden_patterns";

    // === Finom Patterns, Rules, and Styles ===

    [McpServerTool]
    [Description("Returns the standard Finom backend microservice project structure.")]
    public static string ProjectStructure()
        => """
        src/
        ├── {{ServiceName}}/                    # Main service project
        │   ├── Entrypoint.cs                 # Minimal entry point
        │   ├── Startup.cs                    # DI configuration
        │   ├── EQueue.cs                     # Queue enum definitions
        │   ├── Extensions/                   # Service collection extensions
        │   ├── Features/                     # Feature-based organization
        │   │   └── {{FeatureName}}/
        │   │       ├── Background/           # BackgroundService implementations
        │   │       ├── Configs/              # Feature configuration classes
        │   │       ├── Extensions/           # Feature-specific DI extensions
        │   │       ├── MqApi/                # Message queue handlers
        │   │       │   ├── Contracts/        # Message DTOs
        │   │       │   └── MessageHandlers/
        │   │       │       └── Internal/     # Internal message handlers
        │   │       ├── Services/             # Business logic interfaces
        │   │       │   └── Impl/             # Service implementations
        │   │       └── WebApi/               # HTTP API
        │   │           ├── Contracts/        # Request/Response DTOs
        │   │           └── RequestHandlers/  # API endpoints
        │   ├── Infrastructure/               # Cross-cutting concerns
        │   │   └── HttpClients/              # HTTP client configurations
        │   └── Migrations/                   # Database migrations
        ├── {{ServiceName}}.IntegrationTests/   # Integration tests
        │   └── Fixtures/                     # Test fixtures
        ├── {{ServiceName}}.UnitTests/          # Unit tests
        │   └── Features/                     # Feature-mirrored test structure
        └── custom_analyzers/                 # Custom Roslyn analyzers
        """;

    [McpServerTool]
    [Description("Returns Finom naming conventions for C# code elements.")]
    public static string NamingConventions()
        => """
        | Element | Convention | Example |
        |---------|------------|---------|
        | Constants (public/internal) | SCREAMING_SNAKE_CASE | `public const string EXAMPLE_API = "..."` |
        | Constants (private) | SCREAMING_SNAKE_CASE | `private const string DIAGNOSTIC_ID = "..."` |
        | Local constants | SCREAMING_SNAKE_CASE | `const string LOG_TABLE = "..."` |
        | Private fields | _camelCase with underscore prefix | `private readonly IService _service;` |
        | Static members | PascalCase | `public static string Name {{ get; }} ` |
        | Interfaces | IPascalCase | `IHelloStorageService` |
        | Classes | PascalCase | `HelloStorageService` |
        | Methods | PascalCase | `HandleMessage()` |
        | Parameters | camelCase | `HelloLogMessage message` |
        | Properties | PascalCase | `public string Name {{ get; set; }}` |
        """;

    [McpServerTool]
    [Description("Returns Finom C# code style preferences.")]
    public static string CodeStylePreferences()
        => """
        ## General Rules
        - Use `var` everywhere
        - Use primary constructors for DI
        - Use expression-bodied members for simple properties/indexers
        - Use file-scoped namespaces (block-scoped allowed)
        - Use collection expressions: `var items = [1, 2, 3];`
        - Use pattern matching
        - Use null propagation and coalescing
        - Use raw string literals for SQL/multiline
        - Use object initializers
        - Seal classes by default (enforced by custom analyzer SCA001)
        - Mark Startup as sealed
        - Use `readonly` for fields that don't change after construction
        - Prefer `internal` for types not exposed publicly

        ## Examples
        ```csharp
        public sealed class HelloStorageService(IDbMapper dbMapper) : IHelloStorageService
        public string Name => _name;
        var items = [1, 2, 3];
        var name = user?.Name ?? "Anonymous";
        ""
            public sealed class HelloStorageService(IDbMapper dbMapper) : IHelloStorageService
            public string Name => _name;
            var items = [1, 2, 3];
            var name = user?.Name ?? "Anonymous";
        " +
        "INSERT INTO {{M000.LOG_TABLE}} (createdat, name, email)\n" +
        "VALUES (@CreatedAt, @Name, @Email)\n" +
        ""
        ```
        """;

    [McpServerTool]
    [Description("Returns Finom analyzer rules and settings.")]
    public static string AnalyzerRules()
        => """
        ## Custom Analyzers (Enabled)
        - IDA001: Interface member missing XML documentation
        - SCA001: Class should be sealed
        ## Key Microsoft Analyzers (Enabled as Warnings)
        - CA1309/CA1310/CA1311: String comparison rules (use ordinal/culture)
        - CA1507: Use nameof instead of string literal
        - CA1852: Seal internal types
        - CA2016: Forward CancellationToken parameter
        ## StyleCop Rules (Notable Disabled)
        - SA1101: Prefix local calls with this (disabled)
        - SA1309: Field names should not begin with underscore (disabled)
        - SA1600: Elements should be documented (disabled)
        ## IDE Rules (Enabled as Warnings)
        - IDE0005: Remove unnecessary using directives
        - IDE0052: Remove unread private members
        - IDE0060: Remove unused parameter
        - IDE0130: Namespace does not match folder structure
        - IDE0290: Use primary constructor
        ## Build Configuration
        - TreatWarningsAsErrors: true
        - EnforceCodeStyleInBuild: true
        - AnalysisMode: All
        - AnalysisLevel: latest
        """;

    [McpServerTool]
    [Description("Returns Finom testing best practices and patterns.")]
    public static string TestingBestPractices()
        => """
        ## Frameworks & Libraries
        - xUnit for test framework
        - FluentAssertions (v7.2.0) for assertions
        - NSubstitute for mocking
        - AutoBogus for test data generation
        ## Test Structure
        - Unit tests mirror feature structure under `{{ServiceName}}.UnitTests/Features/`
        - Integration tests in `{{ServiceName}}.IntegrationTests/`
        - Use BaseUnitTest as base class for unit tests
        - Use fixtures for database/WebApp integration tests
        ## Naming Convention
        - Test class: `{{ClassName}}Tests`
        - Test method: `MethodName_Scenario_ExpectedResult`
        ## Patterns
        ```csharp
        var contract = new AutoFaker<RequestType>()
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .Generate();
        var result = await _handler.Handle(contract);
        Assert.Equal(expected, result.Property);
        await _dependency.Received(1).Method(Arg.Is<Type>(p => p.Property == value));
        var ex = await Assert.ThrowsAsync<ContractValidationException>(
            () => _handler.Handle(new Request()));
        Assert.Equal("ErrorCode", ex.ExceptionType);
        ```
        """;

    [McpServerTool]
    [Description("Returns Finom logging best practices.")]
    public static string LoggingBestPractices()
        => """
        ## Structured Logging
        logger.Log(config.LogLevel, "[{{Feature}}] Processing {{Name}} with email {{Email}}.", config.FeatureName, name, email);
        ## Config-Driven Log Levels
        logger.Log(config.LogLevel, "Message");
        ## Feature Context Prefix
        "[{{Feature}}] Message description"
        ## Personal Data Protection
        [PersonalData]
        public string? Email {{ get; set; }}
        """;

    [McpServerTool]
    [Description("Returns Finom exception handling patterns.")]
    public static string ExceptionHandlingPatterns()
        => """
        ## Contract Validation Errors
        throw new ContractValidationException("ErrorCode", "User-friendly message");
        ## Custom Web Exceptions with Translation Keys
        throw new CustomWebException("ErrorType")
        {{
            Key = "translation.key.path"  // From PoEditor
        }};
        ## Validation Pattern in Request Handlers
        public override async Task<Response> Handle(Request contract)
        {{
            if (string.IsNullOrWhiteSpace(contract.RequiredField))
            {{
                throw new ContractValidationException("InvalidField", "Field is required");
            }}
            // Continue processing...
        }}
        """;

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
