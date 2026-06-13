Deep Code Base Structural AuditThe repository combines a high-performance C# backend core with a lightweight Node.js/TypeScript gateway. Together, they abstract low-compute model deficiencies by wrapping them in hard-coded runtime verification loops.The architecture, detailed in full in, utilizes a TypeScript gateway (copilot-proxy-tools/) to mimic OpenAI API calls and forward them to a C# .NET Core engine (MonsterTools/). This engine uses AgentLoop.cs to orchestrate ToolExecutor workers (BuildWorker.cs, Searchworkers.cs) to execute deterministic filesystem and build tasks based on local LLM outputs. Comprehensive Architecture Analysis, This repository is designed to act as a Model Context Protocol (MCP) bridging layer tailored for local software development workflows.The primary utility of MonsterTools is to empower low-compute, small footprint local models (such as ibm/granite-4-h-tiny) by offloading complex or multi-step tasks into local, deterministic tools. It coordinates communications between the VS Code Copilot UI, a TypeScript-based Compatibility Proxy, an Agent Loop Orchestration Core, and local tool executors.The local pipeline in this workspace is:

VS Code chat / Copilot-compatible UI
  ↓
copilot-proxy-tools on http://127.0.0.1:5050
  ↓
MonsterTools on http://localhost:5000
  ↓
LM Studio on http://127.0.0.1:1234
  ↓
Deterministic workers

The workspace-level VS Code settings in `.vscode/settings.json` point the local chat endpoint at the proxy on port 5050. That is the setting that keeps the prompt from bypassing MonsterTools and going straight to LM Studio.

How the Multi-Layer Pipeline WorksThe execution lifecycle flow operates linearly across five distinct architectural layers:
[Layer 1: VS Code Copilot UI] 
         ↓ (Standard OpenAI /v1/chat/completions schema payload)
[Layer 2: Copilot Compatibility Proxy (Node.js/TypeScript)]
         ↓ (Transforms formatting & calls local engine /api/agent)
[Layer 3: MonsterTools Execution Engine (C# .NET Core)]
         ↓ (Evaluates tasks using AgentLoop, ToolRouter, & WorkerDispatcher)
[Layer 4: Local Model Runtime (LM Studio)]
         ↓ (Interrogated by the C# engine using custom parameters)
[Layer 5: Deterministic Workers]
           (Executes precise local tasks: builds, file scans, searches)

The Request Interceptor (Layer 1 → Layer 2): The VS Code Copilot UI makes a chat completion request. Instead of routing to the cloud, it hits your local copilot-proxy-tools application.The Schema Normalizer (Layer 2 → Layer 3): The proxy translates open-ended chat queries into structured payloads, streaming back incremental Server-Sent Events (SSE) to the UI while talking to the C# Core Engine.The Brain Loop (Layer 3 → Layer 4): The engine's AgentLoop.cs interrogates LM Studio (LMStudioService.cs) to dynamically break down code intents into structural system calls.The Safe Execution Vector (Layer 3 → Layer 5): ToolRouter.cs maps the requested action, and WorkerDispatcher.cs sends it to targeted background scripts (e.g., BuildWorker.cs, Searchworkers.cs).The Deterministic Return: Instead of allowing the LLM to hallucinate file outputs, code is validated natively, and the raw system-checked tool execution payload travels back up the chain to the user's IDE.



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

CopilotProxy/
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


## Local workspace routing

This workspace includes a `chatLanguageModels.json` file and a `.code-workspace` file that point VS Code chat at the local MonsterTools proxy on `http://127.0.0.1:5050`. Select the `MonsterTools Proxy` model in the chat model picker to keep prompts on the local MonsterTools -> LM Studio -> workers pipeline.
