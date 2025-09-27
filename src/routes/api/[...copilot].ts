import { ensureInternalToken } from '@/shared/api/ensure-internal-token';
import { ensureProxyAuth } from '@/shared/api/ensure-proxy-auth';
import { proxyToCopilot } from '@/shared/api/proxy-to-copilot';
import type { APIEvent } from '@solidjs/start/server';

// Catch-all API route for proxying to Copilot
const ALL = async (event: APIEvent) => {
  // Step 1: optional proxy-level auth (enabled when PROXY_API_KEYS is set)
  const { error: authError } = await ensureProxyAuth(event);
  if (authError) return authError;

  const { bearerToken, error } = await ensureInternalToken(event);
  if (error) return error;

  return proxyToCopilot(event, bearerToken);
};

export const GET = ALL;
export const POST = ALL;
export const PUT = ALL;
export const PATCH = ALL;
export const DELETE = ALL;
