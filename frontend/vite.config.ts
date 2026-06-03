import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      // Optional: uncomment to proxy API calls during dev to avoid CORS issues
      // '/api': { target: 'http://localhost:8080', changeOrigin: true },
      // '/hubs': { target: 'http://localhost:8080', changeOrigin: true, ws: true },
    },
  },
});
