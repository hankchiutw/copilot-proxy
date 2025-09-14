import { getBearerToken } from '@/entities/token/api/copilot-token-meta';
import { getSelectedToken } from '@/entities/token/api/token-storage';
import { log } from '@/shared/lib/logger';
import { maskToken } from '@/shared/lib/mask-token';
import { COPILOT_TOKEN_OVERRIDE_HEADER, PROXY_API_KEYS } from '@/shared/config/config';

const EMPTY_TOKEN = '_';

// Refactored: utility for API routes, not Express middleware
export async function ensureInternalToken(event) {
  const proxyAuthEnabled = PROXY_API_KEYS.length > 0;
  // When proxy auth is enabled, do not parse OAuth token from Authorization header
  // to stay OpenAI-compatible (clients use Authorization for proxy key).
  // Allow override via `x-copilot-token`; otherwise use selected token.
  const rawHeader = proxyAuthEnabled
    ? event.request.headers.get(COPILOT_TOKEN_OVERRIDE_HEADER)
    : event.request.headers.get('authorization');

  const providedToken = rawHeader?.replace(/^(token|Bearer) ?/i, '') || EMPTY_TOKEN;
  const selectedToken = await getSelectedToken();
  const oauthToken = providedToken === EMPTY_TOKEN ? selectedToken?.token : providedToken;

  if (!oauthToken) {
    return {
      error: new Response('Do login or provide a GitHub token in the Authorization header', {
        status: 401,
      }),
    };
  }
  log.info({ 'Use token': maskToken(oauthToken) });

  try {
    const bearerToken = await getBearerToken(oauthToken);
    return { bearerToken };
  } catch (error) {
    log.error(`Error fetching Bearer token from ${oauthToken}: ${error.message}`);
    return { error: new Response(`Internal server error: ${error.message}`, { status: 500 }) };
  }
}
