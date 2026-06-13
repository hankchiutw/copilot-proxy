import { startLogin, startPolling } from '@/entities/token/api/token-auth';
import * as tokenStorage from '@/entities/token/api/token-storage';
import type { APIEvent } from '@solidjs/start/server';

type PromiseFactory = (prevResult?: Record<string, unknown>) => Promise<Record<string, unknown>>;

function createReadableStream(callbacks: Array<PromiseFactory>) {
  const stream = new ReadableStream({
    async start(controller) {
      let prevResult: Record<string, unknown>;
      for (const fn of callbacks) {
        const result = await fn(prevResult);
        controller.enqueue(new TextEncoder().encode(`${JSON.stringify(result)}\n`));
        prevResult = result;
      }

      controller.close();
    },
  });
  return stream;
}

export const POST = async (event: APIEvent) => {
  const stream = createReadableStream([
    startLogin,
    startPolling,
    async (result: { accessToken?: string }) => {
      const { accessToken } = result;
      if (accessToken) {
        await tokenStorage.storeToken({
          name: `Token-${Date.now()}`,
          token: accessToken,
        });
      }
      return result;
    },
  ]);
  return new Response(stream, {
    headers: { 'Content-Type': 'text/plain' },
  });
};
