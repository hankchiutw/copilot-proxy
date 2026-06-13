import { beforeEach, describe, expect, it, vi } from 'vitest';
import { chatCompletionHandler } from './chat-completion-handler';

vi.mock('@solidjs/router', () => ({
  json: vi.fn((data) => ({ json: data })),
}));
vi.mock('langfuse', () => ({
  Langfuse: vi.fn().mockImplementation(() => ({
    trace: vi.fn(() => ({
      update: vi.fn(),
    })),
  })),
  observeOpenAI: vi.fn((client) => client),
}));
let createMock: any;

vi.mock('openai', () => ({
  default: vi.fn().mockImplementation(() => ({
    chat: {
      completions: {
        create: (...args: any[]) => createMock(...args),
      },
    },
  })),
}));
vi.mock('@/shared/lib/logger', () => ({
  log: { error: vi.fn() },
}));
vi.mock('@/shared/lib/mask-token', () => ({
  maskToken: vi.fn((t) => `masked-${t}`),
}));

describe('chatCompletionHandler', () => {
  let config: any;

  beforeEach(() => {
    config = {
      bearerToken: 'test-token',
      headers: new Map([['host', 'api.example.com']]),
      bodyJson: {
        messages: [{ role: 'user', content: 'Hello' }],
        stream: false,
        model: 'gpt-3.5-turbo',
      },
    };
  });

  it('returns JSON response if stream is false', async () => {
    const completions = { id: '123', choices: [] };
    createMock = vi.fn().mockResolvedValue(completions);

    const { json } = await chatCompletionHandler(config);
    expect(json).toEqual(completions);
  });

  it('returns stream response if stream is true', async () => {
    config.bodyJson.stream = true;
    // Simulate async iterable
    const completions = {
      [Symbol.asyncIterator]: async function* () {
        yield { id: 'chunk1' };
        yield { id: 'chunk2' };
      },
    };
    createMock = vi.fn().mockResolvedValue(completions);

    const response = await chatCompletionHandler(config);
    expect(response.status).toBe(200);
    expect(response.headers.get('Content-Type')).toBe('text/event-stream');
    expect(response.body).not.toBeNull();
    // Optionally, test the stream output
    const reader = (response.body as ReadableStream).getReader();
    const decoder = new TextDecoder('utf-8');
    let result = '';
    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      result += decoder.decode(value, { stream: true });
    }
    expect(result).toContain('chunk1');
    expect(result).toContain('chunk2');
  });

  it('logs and throws on OpenAI error', async () => {
    createMock = vi.fn().mockRejectedValue(new Error('fail'));
    await expect(chatCompletionHandler(config)).rejects.toThrow('fail');
  });
});
