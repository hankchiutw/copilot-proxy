import { Router, Request, Response } from 'express';
import { LMStudioAdapter } from '../adapters/LMStudioAdapter';
import { SseHandler } from '../streaming/SseHandler';

const chatRouter = Router();
const adapterInstance = new LMStudioAdapter();

/**
 * POST /v1/chat/completions
 * Intercepts OpenAI-format developer prompts sent by VS Code Copilot.
 */
chatRouter.post('/v1/chat/completions', async (req: Request, res: Response): Promise<void> => {
    try {
        const openAiPayload = req.body;

        // Pass the inbound payload structure through our intelligent routing matrix
        const nodeExecutionStream = await adapterInstance.transformAndRouteRequest(openAiPayload);

        // Stream tokens securely through the line buffer and out to the IDE UI
        SseHandler.streamResponseToClient(res, nodeExecutionStream);
    } catch (routeException: any) {
        console.error('Fatal crash inside proxy router execution path:', routeException.message);
        if (!res.headersSent) {
            res.status(500).json({
                error: 'Internal Gateway Routing Exception',
                message: routeException.message
            });
        }
    }
});

export { chatRouter };
