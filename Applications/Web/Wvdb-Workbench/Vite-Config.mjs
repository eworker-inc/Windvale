import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';
import { defineConfig } from 'vite';

const Projectˉroot = dirname(fileURLToPath(import.meta.url));

export default defineConfig({
  root: Projectˉroot,
  publicDir: resolve(Projectˉroot, 'Public'),
  base: './',
  server: {
    host: '127.0.0.1',
    port: 5182,
    strictPort: true,
    fs: {
      allow: [resolve(Projectˉroot, '../../..')]
    }
  },
  build: {
    outDir: resolve(Projectˉroot, 'Dist'),
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      input: resolve(Projectˉroot, 'index.html')
    }
  }
});
