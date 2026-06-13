import { logHttp } from '@/shared/lib/logger';
import { createMiddleware } from '@solidjs/start/middleware';
import type { FetchEvent } from '@solidjs/start/server';

export default createMiddleware({
  onBeforeResponse: ({ nativeEvent }: FetchEvent) => {
    logHttp(nativeEvent.node.req, nativeEvent.node.res);
  },
});

export const GET = async () =>
    Response.json({
        object:"list",
        data:[
            {
                id:"granite",
                object:"model",
                owned_by:"MonsterTools"
            }
        ]
    });