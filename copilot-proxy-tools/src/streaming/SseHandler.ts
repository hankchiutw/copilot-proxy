import type { Response } from 'express';
import type { Readable } from 'stream';

export class SseHandler {
  static streamResponseToClient(res: Response, upstream: Readable): void {
    res.writeHead(200, {
      'Content-Type': 'text/event-stream',
      'Cache-Control': 'no-cache',
      Connection: 'keep-alive',
      'X-Accel-Buffering': 'no'
    });

    upstream.on('data', chunk => res.write(chunk));
    upstream.on('end', () => res.end());
    upstream.on('error', err => {
      if (!res.headersSent) {
        res.status(500).json({ error: err.message });
      } else {
        res.write(`data: ${JSON.stringify({ error: err.message })}\n\n`);
        res.end();
      }
    });

    res.on('close', () => upstream.destroy());
  }
}
