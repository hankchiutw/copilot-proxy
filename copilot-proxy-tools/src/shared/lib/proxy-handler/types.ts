export interface HandlerConfig {
  bearerToken: string;
  headers: Headers;
  bodyJson: Record<string, any>;
  targetUrl: string;
  targetPath: string;
  request: Request;
}
