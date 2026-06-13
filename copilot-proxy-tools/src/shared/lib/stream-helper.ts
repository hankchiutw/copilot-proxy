import { log } from '@/shared/lib/logger';

export async function readJson(stream: ReadableStream<Uint8Array>) {
  const text = await readText(stream);
  return JSON.parse(text);
}

export async function readText(stream: ReadableStream<Uint8Array>) {
  const reader = stream.getReader();
  const decoder = new TextDecoder();

  let text = '';
  while (true) {
    const { done, value } = await reader.read();
    if (done) break;
    text += decoder.decode(value, { stream: true });
  }
  return text;
}

export async function consumeSSEData(
  stream: ReadableStream<Uint8Array>,
  callback: (data: Record<string, unknown>) => void,
): Promise<void> {
  const reader = stream.getReader();
  const decoder = new TextDecoder();

  let buffer = '';
  while (true) {
    const { done, value } = await reader.read();
    if (done) break;
    buffer += decoder.decode(value, { stream: true });
    const events = buffer.split('\n\n');
    buffer = events.pop() || ''; // incomplete event

    for (const event of events) {
      const dataLine = event.split('\n').find((line) => line.startsWith('data:'));
      if (dataLine) {
        try {
          const data = JSON.parse(dataLine.slice(6));
          callback(data);
        } catch (error) {
          log.warn({ message: error, dataLine }, 'Ignore SSE data:');
        }
      }
    }
  }
}
