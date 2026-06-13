import { randomUUID } from 'node:crypto';

export class OpenAIAdapter {
  static completion(content: string) {
    return {
      id: randomUUID(),
      object: 'chat.completion',
      created: Math.floor(Date.now() / 1000),
      model: 'ibm/granite-4-h-tiny',
      choices: [
        {
          index: 0,
          finish_reason: 'stop',
          message: {
            role: 'assistant',
            content
          }
        }
      ]
    };
  }
}
