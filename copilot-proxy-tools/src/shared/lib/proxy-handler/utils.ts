import { COPILOT_API_HOST, COPILOT_HEADERS } from '@/shared/config/config';
import { log } from '@/shared/lib/logger';
import type { APIEvent } from '@solidjs/start/server';
import { readJson } from '../stream-helper';
import type { HandlerConfig } from './types';

export async function getHandlerConfig(
  event: APIEvent,
  bearerToken: string,
): Promise<HandlerConfig> {
  const url = new URL(event.request.url);
  const targetPath = url.pathname.replace(/^\/api/, '');
  const targetUrl = `https://${COPILOT_API_HOST}${targetPath}${url.search}`;

  // Prepare headers
  const headers = new Headers(event.request.headers);
  COPILOT_HEADERS && Object.entries(COPILOT_HEADERS).forEach(([k, v]) => headers.set(k, v));
  headers.set('authorization', `Bearer ${bearerToken}`);
  headers.set('host', COPILOT_API_HOST);

  // clone body to avoid reading original body stream
  const clonedBody = event.request.clone().body;
  const bodyJson = clonedBody ? await readJson(clonedBody) : {};

  const contentLength = Buffer.byteLength(JSON.stringify(bodyJson), 'utf8');
  headers.set('content-length', contentLength.toString());
  log.debug(
    { headers: Object.fromEntries(headers.entries()), bodyJson },
    'Headers and body for proxying to Copilot',
  );

  const config = {
    bearerToken,
    headers,
    bodyJson,
    targetUrl,
    targetPath,
    request: event.request,
  };

  return config;
}
