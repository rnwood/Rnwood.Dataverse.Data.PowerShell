#!/bin/bash
# Simple test script to verify MCP server starts and responds

cd "$(dirname "$0")"

echo "Building MCP Server..."
dotnet build Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj -c Debug > /dev/null 2>&1

if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "✅ Build succeeded"

echo ""
echo "Starting MCP Server..."
echo "The server will listen on stdin/stdout for MCP protocol messages."
echo ""
echo "To test manually, you can send MCP initialize request:"
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
echo ""
echo "Expected response should include tools: StartScript and GetScriptOutput"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""

# Run the server
dotnet run --project Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj
