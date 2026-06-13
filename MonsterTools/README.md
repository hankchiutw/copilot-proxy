
A deterministic execution layer that augments
small local coding models by converting
high-cost reasoning tasks into reliable tool operations.

(User uses VS Code Gituhub Copilot Interface - LM Studio(ibm/granite-4-h-tiny), LM Studio LLM is set up as a custom endpoint in VS Code Github Copilot)
MCP Service Needs to be compleated and needs to be correctly wired to LM studio so that it is functional.
Will upgrade Workers once i have working pipeline.


\MonsterTools 
  Program.cs
  MonsterTools.csproj
  MonsterMcpServer.cs
\Core
  AgentLoop.cs
  IToolWorker.cs
  ToolCall.cs
  ToolExecutor.cs
  ToolRequest.cs
  ToolResult.cs
  ToolRouter.cs
  ToolSchemas.cs
  ToolWorkerBase.cs
  ToolArgumentNormalizer.cs
  ToolValidator.cs
\Services
 LlmClient.cs
 LMStudioService.cs
 WorkerDispatcher.cs
\Workers
 BuildWorker.cs
 FileSystemWorkers.cs
 FileWorkers.cs
 Searchworkers.cs
 ValidationWorkers.c
 WorkspaceWorker.cs
\obj
  \Debug
  MonsterTools.csproj.nuget.dgspec.json
  MonsterTools.csproj.nuget.g.props
  MonsterTools.csproj.nuget.g.targets
  project.assets.json
  project.nuget.cache



update intergrations to review research then integrate 
Folder Structure For The Proxy

forking the proxy like this:

RCopilotProxy/
│
├── src/
│
├── adapters/
│   ├── CopilotAdapter.ts
│   ├── OpenAIAdapter.ts
│   └── LMStudioAdapter.ts
│
├── middleware/
│   ├── AuthMiddleware.ts
│   ├── LoggingMiddleware.ts
│   └── ContextMiddleware.ts
│
├── routes/
│   ├── ChatRoute.ts
│   ├── CompletionRoute.ts
│   └── ModelsRoute.ts
│
├── integrations/
│   ├── MonsterToolsClient.ts
│   └── WorkspaceClient.ts
│
├── streaming/
│   ├── SseHandler.ts
│   └── ChunkTransformer.ts
│
└── config/
    ├── proxy.json
    └── models.json



    MonsterTools Platform

Layer 1
-------
VS Code Copilot UI

Layer 2
-------
Copilot Compatibility Proxy

Layer 3
-------
MonsterTools Execution Engine

Layer 4
-------
Local Model Runtime (LM Studio)

Layer 5
-------
Deterministic Workers


VS Code Copilot
      ↓
Copilot Proxy
      ↓
POST /api/agent
      ↓
ExecuteAgentEndpoint
      ↓
AgentLoop
      ↓
ToolRouter
      ↓
WorkerDispatcher
      ↓
Worker
      ↓
LM Studio
      ↓
Response
      ↓
Copilot Proxy
      ↓
VS Code