import { chatCompletionHandler, commonHandler, getHandlerConfig } from '@/shared/lib/proxy-handler';
import type { APIEvent } from '@solidjs/start/server';

export async function proxyToCopilot(event: APIEvent, bearerToken: string) {
  const config = await getHandlerConfig(event, bearerToken);

  if (config.targetPath.startsWith('/chat/completions')) {
    return chatCompletionHandler(config);
  }

  return commonHandler(config);
}
