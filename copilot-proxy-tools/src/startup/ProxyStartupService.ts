import fs from 'node:fs';
import path from 'node:path';

export interface ProxyRuntimeConfig {
  proxy: {
    host: string;
    port: number;
  };
  monsterTools: {
    url: string;
  };
  lmStudio: {
    url: string;
    model: string;
    temperature: number;
    stream: boolean;
  };
}

export class ProxyStartupService {
  static loadConfig(baseDir = process.cwd()): ProxyRuntimeConfig {
    const candidatePaths = [
      path.resolve(baseDir, 'src', 'config', 'proxy.json'),
      path.resolve(baseDir, 'config', 'proxy.json'),
      path.resolve(baseDir, 'proxy.json')
    ];

    for (const configPath of candidatePaths) {
      if (!fs.existsSync(configPath)) continue;

      try {
        const raw = fs.readFileSync(configPath, 'utf-8');
        const parsed = JSON.parse(raw) as ProxyRuntimeConfig;

        return {
          proxy: {
            host: parsed.proxy?.host ?? '127.0.0.1',
            port: parsed.proxy?.port ?? 5050
          },
          monsterTools: {
            url: parsed.monsterTools?.url ?? 'http://127.0.0.1:5000'
          },
          lmStudio: {
            url: parsed.lmStudio?.url ?? 'http://127.0.0.1:1234',
            model: parsed.lmStudio?.model ?? 'ibm/granite-4-h-tiny',
            temperature: parsed.lmStudio?.temperature ?? 0,
            stream: parsed.lmStudio?.stream ?? true
          }
        };
      } catch {
        // Try the next candidate path.
      }
    }

    return {
      proxy: { host: '127.0.0.1', port: 5050 },
      monsterTools: { url: 'http://127.0.0.1:5000' },
      lmStudio: {
        url: 'http://127.0.0.1:1234',
        model: 'ibm/granite-4-h-tiny',
        temperature: 0,
        stream: true
      }
    };
  }
}
