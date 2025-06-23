import { COPILOT_API_HOST, COPILOT_HEADERS } from '@/shared/config/config';
import { log } from '@/shared/lib/logger';
import { getSettings } from '@/entities/settings/api/settings-storage';
import type { APIEvent } from '@solidjs/start/server';

const STREAMING_MODELS = [
  'gpt-4.1',
  'gpt-4.1-mini',
  'gpt-4.1-nano',
  'gpt-4.1-2025-04-14',
  'gpt-4.1-mini-2025-04-14',
  'gpt-4.1-nano-2025-04-14',
];

export async function proxyToCopilot(event: APIEvent, bearerToken: string) {
  const settings = await getSettings();
  const url = new URL(event.request.url);
  const targetPath = url.pathname.replace(/^\/api/, '');
  const targetUrl = `https://${COPILOT_API_HOST}${targetPath}${url.search}`;

  // Prepare headers
  const headers = new Headers(event.request.headers);
  COPILOT_HEADERS && Object.entries(COPILOT_HEADERS).forEach(([k, v]) => headers.set(k, v));
  headers.set('authorization', `Bearer ${bearerToken}`);
  headers.set('host', COPILOT_API_HOST);

  let body: BodyInit | null | undefined = undefined;

  if (event.request.method !== 'GET' && event.request.method !== 'HEAD') {
    const contentType = headers.get('content-type') || headers.get('Content-Type');
    if (contentType && contentType.includes('application/json')) {
      const originalJson = await event.request.clone().json().catch(() => null);

      if (originalJson && typeof originalJson === 'object') {
        const model: string | undefined = originalJson.model;
        const requireStreaming = model && STREAMING_MODELS.some((m) => model.startsWith(m));

        if (requireStreaming) {
          if (settings.forceStreamingGpt41) {
            if (!originalJson.stream) {
              originalJson.stream = true;
            }
          } else {
            // Streaming not forced, but we need to ensure client sets it
            if (!originalJson.stream) {
              return new Response(
                JSON.stringify({
                  error:
                    'gpt-4.1 models require \"stream\": true. Enable streaming in your request or turn on "Force streaming" in the web UI.',
                }),
                { status: 400, headers: { 'Content-Type': 'application/json' } },
              );
            }
          }
        }

        body = JSON.stringify(originalJson);
      } else {
        // Fallback to streaming body if JSON parse fails
        body = event.request.body;
      }
    } else {
      // Non-JSON, forward as is
      body = event.request.body;
    }
  }

  if (event.request.method === 'GET' || event.request.method === 'HEAD') {
    body = undefined;
  }

  log.info(`Proxying to: ${event.request.method} ${targetUrl}`);

  // Proxy the request
  const proxyResponse = await fetch(targetUrl, {
    method: event.request.method,
    headers,
    body,
  });

  log.info(`Proxy response: ${proxyResponse.status} ${proxyResponse.statusText}`);

  // Return proxied response
  return new Response(proxyResponse.body, {
    status: proxyResponse.status,
    headers: proxyResponse.headers,
  });
}
