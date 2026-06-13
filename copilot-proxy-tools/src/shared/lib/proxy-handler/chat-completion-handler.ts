import { json as responseJson } from '@solidjs/router';
import { Langfuse, observeOpenAI } from 'langfuse';
import OpenAI from 'openai';
import type { HandlerConfig } from './types';

import { log } from '@/shared/lib/logger';
import { maskToken } from '@/shared/lib/mask-token';

const langfuse = new Langfuse();

async function responseStream(completions: AsyncIterable<OpenAI.ChatCompletion>) {
  const encoder = new TextEncoder();
  const stream = new ReadableStream({
    async start(controller) {
      for await (const chunk of completions) {
        controller.enqueue(encoder.encode(`data: ${JSON.stringify(chunk)}\n\n`));
      }
      controller.close();
    },
  });

  return new Response(stream, {
    status: 200,
    headers: {
      'Content-Type': 'text/event-stream',
    },
  });
}

export async function chatCompletionHandler(config: HandlerConfig) {
  const { bearerToken, headers, bodyJson } = config;
  const trace = langfuse.trace({
    name: 'copilot-proxy-chat-completions',
    input: bodyJson.messages,
    metadata: {
      maskedToken: maskToken(bearerToken),
      stream: bodyJson.stream,
      model: bodyJson.model,
    },
  });

  const client = new OpenAI({
    apiKey: bearerToken,
    baseURL: `https://${headers.get('host')}`,
    fetchOptions: {
      headers,
    } as any,
  });
  const wrappedClient = observeOpenAI(client, {
    parent: trace,
    generationName: 'proxy-chat-generation',
  });

  const completions = await wrappedClient.chat.completions
    .create(bodyJson as OpenAI.ChatCompletionCreateParams)
    .catch((e) => {
      log.error(e, 'OpenAI chat completions error');
      throw e;
    });

  if (!bodyJson.stream) {
    trace.update({ output: completions });
    return responseJson(completions);
  }
  return responseStream(completions as AsyncIterable<OpenAI.ChatCompletion>);
}
