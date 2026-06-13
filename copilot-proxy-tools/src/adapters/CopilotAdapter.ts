export interface CopilotMessage {
  role: 'system' | 'user' | 'assistant';
  content: string;
}

export interface CopilotPayload {
  messages?: CopilotMessage[];
  input?: string | Array<{ role?: string; content?: string }>;
  prompt?: string;
  conversation?: Array<{ role?: string; content?: string }>;
  model?: string;
  stream?: boolean;
  workspaceRoot?: string;
}

export interface MonsterAgentRequest {
  prompt: string;
  targetModel?: string;
  arguments?: Record<string, unknown>;
  workspace?: string;
}

export class CopilotAdapter {
  static extractPrompt(body: CopilotPayload): string {
    const messageBlocks = body.messages ?? body.conversation ?? [];
    if (messageBlocks.length > 0) {
      const joined = messageBlocks
        .map(message => message?.content ?? '')
        .filter(Boolean)
        .join('\n');
      if (joined.trim()) return joined;
    }

    if (typeof body.input === 'string' && body.input.trim()) {
      return body.input.trim();
    }

    if (Array.isArray(body.input)) {
      const joined = body.input
        .map(message => message?.content ?? '')
        .filter(Boolean)
        .join('\n');
      if (joined.trim()) return joined;
    }

    if (typeof body.prompt === 'string' && body.prompt.trim()) {
      return body.prompt.trim();
    }

    return '';
  }

  static toMonsterToolsRequest(body: CopilotPayload): MonsterAgentRequest {
    const prompt = CopilotAdapter.extractPrompt(body);
    const workspace = body.workspaceRoot ?? process.cwd();

    return {
      prompt,
      targetModel: body.model ?? 'ibm/granite-4-h-tiny',
      arguments: {
        workspace
      },
      workspace
    };
  }

  static shouldUseMonsterTools(_prompt: string): boolean {
    return true;
  }
}
