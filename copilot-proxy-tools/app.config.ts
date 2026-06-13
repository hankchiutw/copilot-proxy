import { defineConfig } from '@solidjs/start/config';
import tailwindcss from '@tailwindcss/vite';

import pkg from './package.json';

export default defineConfig({
  vite: {
    define: {
      __APP_VERSION__: JSON.stringify(pkg.version),
    },
    plugins: [tailwindcss()],
    resolve: {
      alias: {
        '@': '/src',
      },
    },
  },
  middleware: 'src/middleware/index.ts',
  server: {
    output: {
      dir: 'dist',
      serverDir: 'dist/server',
      publicDir: 'dist/public',
    },
    esbuild: {
      options: {
        supported: {
          'top-level-await': true,
        },
      },
    },
  },
});
