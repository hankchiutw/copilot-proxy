import storage from 'node-persist';
import { v4 as uuid } from 'uuid';
import type { TokenStorageItem } from '../model/types';

const STORAGE_DIR = process.env.STORAGE_DIR || '.storage';

const tokenStorage = storage.create({
  dir: `${STORAGE_DIR}/tokens`,
  ttl: false, // tokens never expire by default
});

const selectedTokenStorage = storage.create({
  dir: `${STORAGE_DIR}/selected-token`,
  ttl: false, // tokens never expire by default
});

await tokenStorage.init();
await selectedTokenStorage.init();

// Store a new token: { name, token }
export async function storeToken({
  id,
  name,
  token,
}: { id?: string; name: string; token: string }): Promise<TokenStorageItem> {
  const item = id
    ? await tokenStorage.getItem(id)
    : { id: uuid(), name, token, createdAt: Date.now() };
  await tokenStorage.setItem(item.id, { ...item, name, token });
  return await tokenStorage.getItem(item.id);
}

export async function getToken(id: TokenStorageItem['id']): Promise<TokenStorageItem | undefined> {
  return await tokenStorage.getItem(id);
}

export async function updateName(id: TokenStorageItem['id'], name: TokenStorageItem['name']) {
  const item = await tokenStorage.getItem(id);
  if (!item) {
    throw new Error('Token not found');
  }
  await tokenStorage.setItem(item.id, { ...item, name });
}

export async function updateMetaByToken(oauthToken: string, meta: TokenStorageItem['meta']) {
  const tokens = await tokenStorage.values();
  const tokenItem = tokens.find((item) => item.token === oauthToken);
  if (!tokenItem) {
    // may be a user-provided token, do nothing
    return;
  }
  await tokenStorage.setItem(tokenItem.id, { ...tokenItem, meta });
}

// Get all tokens
export async function getTokens(): Promise<TokenStorageItem[]> {
  return (await tokenStorage.values()) || [];
}

// Remove a token by id
export async function removeToken(id: TokenStorageItem['id']) {
  await tokenStorage.removeItem(id);
}

export async function selectToken(id: TokenStorageItem['id']) {
  await selectedTokenStorage.setItem('selected-token-id', id);
}

export async function getSelectedToken(): Promise<TokenStorageItem | null> {
  const id = await selectedTokenStorage.getItem('selected-token-id');
  if (!id) {
    return null;
  }
  return await tokenStorage.getItem(id);
}
