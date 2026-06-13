import { refreshMeta } from '@/entities/token/api/copilot-token-meta';
import * as tokenStorage from '@/entities/token/api/token-storage';
import { maskToken } from '@/shared/lib/mask-token';
import { action, query, revalidate } from '@solidjs/router';
import type { TokenItem, TokenStorageItem } from './types';

function transformTokenItem(
  tokenItem: TokenStorageItem,
  defaultToken?: TokenStorageItem,
): TokenItem {
  return {
    ...tokenItem,
    token: maskToken(tokenItem.token),
    default: defaultToken && defaultToken.id === tokenItem.id,
  };
}

export const getTokenList = query(async () => {
  'use server';
  const tokens = await tokenStorage.getTokens();
  const defaultToken = await tokenStorage.getSelectedToken();
  const results = tokens
    .map((item) => transformTokenItem(item, defaultToken))
    .sort((a, b) => b.createdAt - a.createdAt);

  return results;
}, 'tokens');

export const refetchTokenList = () => revalidate(getTokenList.key);

export const setDefaultToken = action(async (id: string) => {
  'use server';
  await tokenStorage.selectToken(id);
}, 'setDefaultToken');

export const removeToken = action(async (id: string) => {
  'use server';
  await tokenStorage.removeToken(id);
}, 'removeToken');

export const renameToken = action(async (id: string, name: string) => {
  'use server';
  await tokenStorage.updateName(id, name);
}, 'renameToken');

export const refreshTokenMeta = action(async (id: string) => {
  'use server';
  const { token } = await tokenStorage.getToken(id);
  const meta = await refreshMeta(token);
  await tokenStorage.updateMetaByToken(token, meta);
  return meta;
}, 'refreshTokenMeta');

export const addToken = action(async (name: string, token: string): Promise<TokenItem | null> => {
  'use server';

  if (!name || !token) {
    return null;
  }
  try {
    const item = await tokenStorage.storeToken({ name, token });
    return transformTokenItem(item);
  } catch (e) {
    return null;
  }
}, 'addToken');
