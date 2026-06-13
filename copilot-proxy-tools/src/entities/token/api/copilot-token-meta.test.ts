import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('@/shared/config/config', () => ({
  COPILOT_TOKEN_API_URL: 'https://api.example.com/token',
}));

vi.mock('./token-storage', () => ({
  updateMetaByToken: vi.fn(),
}));

import { getBearerToken, refreshMeta } from './copilot-token-meta';
import * as tokenStorage from './token-storage';

const mockFetchResponse = {
  token: 'bearer-token-123',
  expires_at: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
  limited_user_quotas: { chat: 42, completions: 99 },
  limited_user_reset_date: Math.floor(Date.now() / 1000) + 3600,
};

describe('copilot-token-meta', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => mockFetchResponse,
    }) as any;
  });

  it('refreshMeta fetches and updates meta', async () => {
    const meta = await refreshMeta('oauth-abc');
    expect(meta.token).toBe('bearer-token-123');
    expect(tokenStorage.updateMetaByToken).toHaveBeenCalledWith(
      'oauth-abc',
      expect.objectContaining({
        token: 'bearer-token-123',
        chatQuota: 42,
        completionsQuota: 99,
      }),
    );
  });

  it('getBearerToken returns token and caches it', async () => {
    const token = await getBearerToken('oauth-abc');
    expect(token).toBe('bearer-token-123');
    // Second call should use cache, so fetch should not be called again
    (globalThis.fetch as any).mockClear();
    const token2 = await getBearerToken('oauth-abc');
    expect(token2).toBe('bearer-token-123');
    expect(globalThis.fetch).not.toHaveBeenCalled();
  });

  it('getBearerToken refreshes if token is expired', async () => {
    // 1. First, set up a fetch response with an expired token
    const expiredFetchResponse = {
      ...mockFetchResponse,
      expires_at: new Date(Date.now() - 60 * 60 * 1000).toISOString(), // expired 1 hour ago
    };
    (globalThis.fetch as any).mockResolvedValueOnce({
      ok: true,
      json: async () => expiredFetchResponse,
    });

    // 2. Call refreshMeta to cache the expired token
    await refreshMeta('oauth-abc');

    // 3. Now, set up fetch to be called again for the refresh (with a valid token)
    (globalThis.fetch as any).mockResolvedValueOnce({
      ok: true,
      json: async () => mockFetchResponse,
    });

    // 4. Spy on fetch
    const fetchSpy = vi.spyOn(globalThis, 'fetch');

    // 5. Call getBearerToken, which should detect expiry and call fetch again
    await getBearerToken('oauth-abc');
    expect(fetchSpy).toHaveBeenCalled();

    fetchSpy.mockRestore();
  });

  it('throws if fetch fails', async () => {
    (globalThis.fetch as any).mockResolvedValueOnce({
      ok: false,
      status: 401,
      statusText: 'Unauthorized',
    });
    await expect(refreshMeta('bad-token')).rejects.toThrow(
      'Failed to fetch token: 401 Unauthorized',
    );
  });

  it('handles null limited_user_quotas and limited_user_reset_date', async () => {
    (globalThis.fetch as any).mockResolvedValueOnce({
      ok: true,
      json: async () => ({
        token: 'bearer-token-xyz',
        expires_at: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
        limited_user_quotas: null,
        limited_user_reset_date: null,
      }),
    });

    const meta = await refreshMeta('oauth-null');
    expect(meta.token).toBe('bearer-token-xyz');
    expect(meta.chatQuota).toBeNull();
    expect(meta.completionsQuota).toBeNull();
    expect(meta.resetTime).toBeNull();
  });
});
