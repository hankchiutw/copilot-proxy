import { PROXY_API_KEYS } from '@/shared/config/config';
import { log } from '@/shared/lib/logger';

function parseBearer(authorizationHeader?: string | null): string | null {
  if (!authorizationHeader) return null;
  const m = authorizationHeader.match(/^(?:Bearer|token)\s+(.+)$/i);
  return m ? m[1] : null;
}

export async function ensureProxyAuth(event: { request: Request }) {
  if (!PROXY_API_KEYS.length) {
    // Auth disabled -> allow all
    return { ok: true } as const;
  }

  const provided = parseBearer(event.request.headers.get('authorization'));
  const ok = !!provided && PROXY_API_KEYS.includes(provided);
  if (!ok) {
    log.warn('Unauthorized proxy access attempt');
    return {
      error: new Response('Unauthorized', {
        status: 401,
        headers: { 'WWW-Authenticate': 'Bearer realm="copilot-proxy"' },
      }),
    } as const;
  }
  return { ok: true } as const;
}

