import { Readable } from 'node:stream';
import { CopilotAdapter, type CopilotPayload } from './CopilotAdapter.js';
import { MonsterToolsClient } from '../integrations/MonsterToolsClient.js';

export class LMStudioAdapter {
  constructor(
    private readonly monsterTools = new MonsterToolsClient('http://127.0.0.1:5000'),
    private readonly defaultModel = 'ibm/granite-4-h-tiny'
  ) {}

  async transformAndRouteRequest(payload: CopilotPayload): Promise<Readable> {
    const prompt = CopilotAdapter.extractPrompt(payload).trim();
    const workspace = payload.workspaceRoot ?? process.cwd();

    if (!prompt) {
      console.warn('[Proxy] No prompt found in incoming payload. Check the VS Code endpoint mapping.');
      return this.toSseStream('No prompt supplied.');
    }

    try {
      console.log(`[Proxy] Routing prompt through MonsterTools: ${prompt.slice(0, 120)}`);

      const result = await this.monsterTools.dispatchAgentTask({
        prompt,
        workspaceRoot: workspace
      });

      const responseText = (result.output || result.error || 'No execution output returned from MonsterTools.').trim();
      return this.toSseStream(responseText);
    } catch (error: any) {
      return this.toSseStream(`MonsterTools routing failed: ${error.message}`);
    }
  }

  private toSseStream(text: string): Readable {
    const stream = new Readable({
      read() {}
    });

    const chunk = {
      id: `chatcmpl-${Math.random().toString(36).slice(2, 10)}`,
      object: 'chat.completion.chunk',
      created: Math.floor(Date.now() / 1000),
      model: this.defaultModel,
      choices: [
        {
          index: 0,
          delta: { content: text },
          finish_reason: 'stop'
        }
      ]
    };

    stream.push(`data: ${JSON.stringify(chunk)}\n\n`);
    stream.push('data: [DONE]\n\n');
    stream.push(null);
    return stream;
  }
}
