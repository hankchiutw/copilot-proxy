import { log } from '@/shared/lib/logger';
import type { HandlerConfig } from './types';

export async function commonHandler(config: HandlerConfig) {
  const { headers, request, targetUrl } = config;
  const body = request.method === 'GET' || request.method === 'HEAD' ? undefined : request.body;

  log.info(`Proxying to: ${request.method} ${targetUrl}`);

  // Proxy the request
  const proxyResponse = await fetch(targetUrl, {
    method: request.method,
    headers,
    body,
    duplex: 'half',
  });

  log.info(`Proxy response: ${proxyResponse.status} ${proxyResponse.statusText}`);

  return proxyResponse;
}
