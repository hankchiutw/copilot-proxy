import { log } from '@/shared/lib/logger';

const CLIENT_ID = 'Iv1.b507a08c87ecfe98';
const MANDATORY_HEADERS = Object.freeze({
  accept: 'application/json',
  'editor-version': 'Neovim/0.6.1',
  'editor-plugin-version': 'copilot.vim/1.16.0',
  'content-type': 'application/json',
  'user-agent': 'GithubCopilot/1.155.0',
  'accept-encoding': 'gzip,deflate,br',
});

const state = {
  accessToken: null,
  deviceCode: null,
  userCode: null,
  verificationUri: null,
  expiresAt: null,
  polling: false,
  message: 'Not logged in.',
};

let pollingPromise = null;

function resetWithMessage(message) {
  state.accessToken = null;
  state.deviceCode = null;
  state.userCode = null;
  state.verificationUri = null;
  state.expiresAt = null;
  state.polling = false;
  state.message = message;

  pollingPromise = null;
}

async function startDeviceLogin() {
  if (state.polling) return state;

  const resp = await fetch('https://github.com/login/device/code', {
    method: 'POST',
    headers: MANDATORY_HEADERS,
    body: JSON.stringify({ client_id: CLIENT_ID, scope: 'read:user' }),
  });
  const json = await resp.json();

  resetWithMessage('Waiting for user to authorize the device.');
  Object.assign(state, {
    deviceCode: json.device_code,
    userCode: json.user_code,
    verificationUri: json.verification_uri,
    expiresAt: Date.now() + (json.expires_in || 900) * 1000,
    polling: true,
  });

  pollingPromise = pollForToken();

  return state;
}

async function pollForToken() {
  while (state.polling) {
    log.info(state, 'Polling for token for device code');
    if (Date.now() > state.expiresAt) {
      resetWithMessage('Device code expired.');
      return state;
    }

    await new Promise((r) => setTimeout(r, 5000));
    const resp = await fetch('https://github.com/login/oauth/access_token', {
      method: 'POST',
      headers: MANDATORY_HEADERS,
      body: JSON.stringify({
        client_id: CLIENT_ID,
        device_code: state.deviceCode,
        grant_type: 'urn:ietf:params:oauth:grant-type:device_code',
      }),
    });
    const json = await resp.json();

    if (json.access_token) {
      resetWithMessage('Login successful.');
      state.accessToken = json.access_token;
      return state;
    }
    if (json.error === 'authorization_pending') {
      // Still in process, continue polling.
    } else if (json.error) {
      resetWithMessage(`Error: ${json.error_description || json.error}`);
      return state;
    }
  }
}

export async function startLogin() {
  return startDeviceLogin();
}

export async function startPolling() {
  if (pollingPromise) return pollingPromise;
  pollingPromise = pollForToken();
  return pollingPromise;
}
