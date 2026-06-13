import { logHttp } from '@/shared/lib/logger';
import { createMiddleware } from '@solidjs/start/middleware';
import type { FetchEvent } from '@solidjs/start/server';

export interface RequestContext {
    workspace:string;
}

export async function ContextMiddleware(
    request:Request
):Promise<RequestContext> {

    return {
        workspace:
            WorkspaceClient.resolve(
                request
            )
    };
}
