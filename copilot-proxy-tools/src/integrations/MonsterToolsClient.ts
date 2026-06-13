import axios, { AxiosInstance } from 'axios';

export interface MonsterAgentRequest {
  prompt: string;
  workspaceRoot?: string;
}

export interface MonsterAgentResponse {
  success: boolean;
  output: string;
  error?: string;
}

export class MonsterToolsClient {
  private httpClient: AxiosInstance;
  private readonly defaultBackendUrl = 'http://127.0.0.1:5000';

  constructor(baseURL?: string) {
    this.httpClient = axios.create({
      baseURL: baseURL || this.defaultBackendUrl,
      timeout: 120000,
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
        'X-Client-Source': 'Copilot-Compatibility-Proxy'
      }
    });
  }

  public async dispatchAgentTask(payload: MonsterAgentRequest): Promise<MonsterAgentResponse> {
    try {
      const normalizedPayload = {
        prompt: payload.prompt,
        workspace: payload.workspaceRoot || null
      };

      console.log(
        `[MonsterToolsClient] Dispatching prompt to ${this.httpClient.defaults.baseURL}: ${payload.prompt.slice(0, 120)}`
      );

      const response = await this.httpClient.post<MonsterAgentResponse>(
        '/api/agent',
        normalizedPayload
      );

      return response.data;
    } catch (error: any) {
      this.handleNetworkError(error, 'dispatchAgentTask');
      throw error;
    }
  }

  public async checkBackendHealth(): Promise<{ status: string; upstreamOnline: boolean }> {
    try {
      const response = await this.httpClient.get<any>('/health');
      const data = response.data ?? {};
      const isUpstreamHealthy = Boolean(data.lmStudio ?? data.lmstudio ?? data.upstreamOnline ?? false);

      return {
        status: isUpstreamHealthy ? 'Healthy' : 'Degraded',
        upstreamOnline: isUpstreamHealthy
      };
    } catch {
      return {
        status: 'Unreachable',
        upstreamOnline: false
      };
    }
  }

  private handleNetworkError(error: any, context: string): void {
    if (error.response) {
      console.error(
        `[MonsterToolsClient][${context}] Backend rejected payload with status ${error.response.status}:`,
        error.response.data
      );
    } else if (error.request) {
      console.error(
        `[MonsterToolsClient][${context}] No response received from C# engine at ${this.httpClient.defaults.baseURL}.`
      );
    } else {
      console.error(`[MonsterToolsClient][${context}] Pipeline setup error:`, error.message);
    }
  }
}
