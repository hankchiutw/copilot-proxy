import express from 'express';
import { SseHandler } from './streaming/SseHandler.js';

const app = express();

app.post('/api/chat/stream', async (req, res) => {
  await SseHandler.handleStream(req, res, async () => {
    // Replace with your actual configuration setup (e.g. from config/proxy.json)
    const lmStudioResponse = await fetch('http://localhost:1234/v1/chat/completions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        model: 'ibm/granite-4-h-tiny',
        messages: req.body.messages,
        stream: true
      })
    });

    if (!lmStudioResponse.body) {
      throw new Error('No readable body returned from LM Studio');
    }

    // Converts Web ReadableStream to a Node.js Readable stream compatibility layer
    return Readable.fromWeb(lmStudioResponse.body as any);
  });
});
