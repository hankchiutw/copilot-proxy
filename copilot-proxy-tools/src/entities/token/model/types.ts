export interface TokenAuth {
  message: string;
  accessToken?: string;
  deviceCode?: string;
  userCode?: string;
  verificationUri?: string;
}

export interface TokenItem extends TokenStorageItem {
  default: boolean;
}

export interface TokenStorageItem {
  id: string;
  name: string;
  token: string;
  createdAt: number;
  meta?: CopilotMeta;
}

export interface CopilotMeta {
  token: string;
  expiresAt: number;
  resetTime: number | null;
  chatQuota: number | null;
  completionsQuota: number | null;
}
