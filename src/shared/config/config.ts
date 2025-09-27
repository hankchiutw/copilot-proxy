export const COPILOT_TOKEN_API_URL = 'https://api.github.com/copilot_internal/v2/token';
export const COPILOT_API_HOST = 'api.githubcopilot.com';
export const COPILOT_HEADERS = {
  'Editor-Version': 'CopilotProxy/0.1.0',
  'Copilot-Integration-Id': 'vscode-chat',
  'Copilot-Vision-Request': 'true',
  'User-Agent': 'CopilotProxy',
};

// Comma-separated list of API keys required for accessing the local OpenAI-compatible proxy.
// If empty, proxy-level authentication is disabled (backward compatible).
export const PROXY_API_KEYS = (process.env.PROXY_API_KEYS || '')
  .split(',')
  .map((s) => s.trim())
  .filter(Boolean);

// Header name for overriding Copilot OAuth token when proxy auth is enabled.
export const COPILOT_TOKEN_OVERRIDE_HEADER = 'x-copilot-token';
