import path from 'path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    outDir: '../PacoDemo.Api/wwwroot',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5000',
      '/hubs': {
        target: 'http://localhost:5000',
        ws: true,
      },
    },
  },
})
