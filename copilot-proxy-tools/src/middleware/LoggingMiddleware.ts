import { log } from "@/shared/lib/logger";

export async function LoggingMiddleware(
    request: Request
) {
    log.info({
        method: request.method,
        url: request.url
    });
}