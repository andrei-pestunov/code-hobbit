# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY src/CodeHobbit.sln .
COPY src/CodeHobbit.McpServer/CodeHobbit.McpServer.csproj CodeHobbit.McpServer/
COPY src/CodeHobbit.Rag/CodeHobbit.Rag.csproj CodeHobbit.Rag/
COPY src/custom_analyzers/CustomAnalyzers.csproj custom_analyzers/
COPY Directory.Build.props .

# Restore dependencies
RUN dotnet restore CodeHobbit.sln

# Copy all source code
COPY src/ .

# Build and publish
RUN dotnet publish CodeHobbit.McpServer/CodeHobbit.McpServer.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-preview AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

USER appuser

# Default environment variables
ENV QDRANT_ENDPOINT=http://qdrant:6333
ENV RAG_REPO_PATH=/data/repo

# Expose volume for repository data
VOLUME ["/data/repo"]

ENTRYPOINT ["./CodeHobbit.McpServer"]
