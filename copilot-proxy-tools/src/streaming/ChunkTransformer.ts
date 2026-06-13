import { Transform, TransformCallback } from 'stream';

type SSEPayload = {
  choices?: Array<{
    delta?: { content?: string };
    finish_reason?: string | null;
  }>;
};

export class ChunkTransformer extends Transform {
  private buffer = '';

  constructor() {
    super({ readableObjectMode: true });
  }

  override _transform(chunk: any, encoding: BufferEncoding, callback: TransformCallback): void {
    try {
      this.buffer += Buffer.isBuffer(chunk) ? chunk.toString('utf8') : String(chunk);

      while (true) {
        const boundary = this.buffer.indexOf('\n\n');
        if (boundary === -1) break;

        const block = this.buffer.slice(0, boundary).replace(/\r/g, '');
        this.buffer = this.buffer.slice(boundary + 2);

        if (!block.trim()) continue;

        const line = block.split('\n').find(l => l.startsWith('data:'));
        if (!line) continue;

        const data = line.slice(5).trim();
        if (data === '[DONE]') {
          this.push({ done: true, raw: 'data: [DONE]\n\n' });
          continue;
        }

        if (!data.startsWith('{') || !data.endsWith('}')) {
          this.buffer = `${block}\n\n${this.buffer}`;
          break;
        }

        try {
          const parsed = JSON.parse(data) as SSEPayload;
          const content = parsed.choices?.[0]?.delta?.content ?? '';
          this.push({
            done: false,
            data: parsed,
            raw: content ? `data: ${JSON.stringify(parsed)}\n\n` : ''
          });
        } catch {
          this.buffer = `${block}\n\n${this.buffer}`;
          break;
        }
      }

      callback();
    } catch (error) {
      callback(error as Error);
    }
  }

  override _flush(callback: TransformCallback): void {
    this.buffer = '';
    callback();
  }
}
