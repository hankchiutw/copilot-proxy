import express, { type NextFunction, type Request, type Response } from 'express';
import { LMStudioAdapter } from './adapters/LMStudioAdapter.js';
import { SseHandler } from './streaming/SseHandler.js';
import { ProxyStartupService } from './startup/ProxyStartupService.js';
import { MonsterToolsClient } from './integrations/MonsterToolsClient.js';

const config = ProxyStartupService.loadConfig();
const app = express();

const monsterTools = new MonsterToolsClient(config.monsterTools.url);
const adapter = new LMStudioAdapter(
  monsterTools,
  config.lmStudio.model
);

app.use(express.json({ limit: '8mb' }));
app.use(express.urlencoded({ extended: true }));

app.use((req, res, next) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET,POST,PUT,PATCH,DELETE,OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, X-Workspace-Root, X-Workspace-Root-Path');
  if (req.method === 'OPTIONS') {
    return res.sendStatus(204);
  }
  next();
});

app.use((req, _res, next) => {
  if (req.method !== 'GET') {
    console.log(`[Proxy] ${req.method} ${req.originalUrl}`);
  }
  next();
});

app.get('/health', async (_req: Request, res: Response) => {
  const backend = await monsterTools.checkBackendHealth();

  res.json({
    ok: true,
    service: 'copilot-proxy-tools',
    proxy: config.proxy,
    monsterTools: {
      url: config.monsterTools.url,
      health: backend
    },
    lmStudio: config.lmStudio,
    routes: {
      chat: '/v1/chat/completions',
      completions: '/v1/completions',
      responses: '/v1/responses',
      agent: '/api/agent'
    }
  });
});

app.get('/v1/models', (_req: Request, res: Response) => {
  res.json({
    object: 'list',
    data: [
      {
        id: config.lmStudio.model,
        object: 'model',
        owned_by: 'local',
        supports_tools: true,
        supports_streaming: true
      }
    ]
  });
});

const handleCompletion = async (req: Request, res: Response) => {
  try {
    const workspaceRoot =
      req.header('x-workspace-root') ??
      req.header('x-workspace-root-path') ??
      req.body?.workspaceRoot ??
      req.body?.workspace ??
      undefined;

    if (workspaceRoot) {
      console.log(`[Proxy] Workspace root: ${workspaceRoot}`);
    }

    const stream = await adapter.transformAndRouteRequest({
      ...(req.body ?? {}),
      workspaceRoot
    });
    SseHandler.streamResponseToClient(res, stream);
  } catch (error: any) {
    res.status(500).json({ error: error?.message ?? 'Unknown proxy error' });
  }
};

const completionRoutes = [
  '/v1/chat/completions',
  '/v1/completions',
  '/v1/responses',
  '/api/chat/completions',
  '/api/completions',
  '/api/responses',
  '/chat/completions',
  '/completions',
  '/responses'
];

completionRoutes.forEach(route => {
  app.post(route, handleCompletion);
});

// Graceful JSON parsing error handling so malformed client payloads do not crash the server.
app.use((err: any, _req: Request, res: Response, next: NextFunction) => {
  if (err?.type === 'entity.parse.failed' || err instanceof SyntaxError) {
    return res.status(400).json({
      error: 'Invalid JSON payload.',
      details: err.message
    });
  }

  next(err);
});

app.listen(config.proxy.port, config.proxy.host, () => {
  console.log(`copilot-proxy-tools listening on http://${config.proxy.host}:${config.proxy.port}`);
  console.log(`MonsterTools -> ${config.monsterTools.url}`);
  console.log(`LM Studio -> ${config.lmStudio.url}`);
});
