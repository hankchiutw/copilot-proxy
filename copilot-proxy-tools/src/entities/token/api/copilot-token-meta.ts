import { updateMetaByToken } from '@/entities/token/api/token-storage';
import { COPILOT_TOKEN_API_URL } from '@/shared/config/config';
import type { CopilotMeta } from '../model/types';

interface CopilotApiResponse {
  token: string;
  expires_at: string;
  limited_user_quotas: {
    chat: number;
    completions: number;
  } | null;
  limited_user_reset_date: number | null;
}

// Token cache storing {token, expiresAt} keyed by oauthToken
const cacheMap = new Map<string, CopilotMeta>();

// Function to check if a cached token is still valid (with 5 min buffer)
const isTokenValid = (meta?: CopilotMeta): boolean => {
  if (!meta) return false;
  const bufferDuration = 5 * 60 * 1000; // 5 minutes
  return Date.now() < meta.expiresAt - bufferDuration;
};

async function fetchMeta(oauthToken: string): Promise<CopilotMeta> {
  const res = await fetch(COPILOT_TOKEN_API_URL, {
    method: 'GET',
    headers: {
      'User-Agent': 'CopilotProxy',
      Authorization: `token ${oauthToken}`,
    },
  });

  if (!res.ok) {
    throw new Error(`Failed to fetch token: ${res.status} ${res.statusText}`);
  }

  const { token, expires_at, limited_user_quotas, limited_user_reset_date }: CopilotApiResponse =
    await res.json();

  const chatQuota = limited_user_quotas?.chat ?? null;
  const completionsQuota = limited_user_quotas?.completions ?? null;
  const resetTime =
    limited_user_reset_date !== null && limited_user_reset_date !== undefined
      ? limited_user_reset_date * 1000
      : null;
  const expiresAt = expires_at ? new Date(expires_at).getTime() : Date.now() + 60 * 60 * 1000;

  return { token, expiresAt, resetTime, chatQuota, completionsQuota };
}

export async function refreshMeta(oauthToken: string): Promise<CopilotMeta> {
  const meta = await fetchMeta(oauthToken);

  cacheMap.set(oauthToken, meta);
  await updateMetaByToken(oauthToken, meta);
  return meta;
}

export async function getBearerToken(oauthToken: string): Promise<string> {
  let meta = cacheMap.get(oauthToken);
  if (!isTokenValid(meta)) {
    meta = await refreshMeta(oauthToken);
  }
  return meta?.token || '';
}
