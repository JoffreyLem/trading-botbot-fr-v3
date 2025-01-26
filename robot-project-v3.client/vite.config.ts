import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import plugin from "@vitejs/plugin-react";

const target = process.env.API_URL || "https://localhost:7111/";
const secure = process.env.SECURE === "true";

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    target: "esnext",
  },
  plugins: [plugin()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    proxy: {
      "^/api/ApiProvider": {
        target,
        secure,
        changeOrigin: true,
      },
      "^/api/Strategy": {
        target,
        secure,
        changeOrigin: true,
      },
      "^/api/StrategyGenerator": {
        target,
        secure,
        changeOrigin: true,
      },
      "^/infoClient": {
        target,
        ws: true,
        secure,
        changeOrigin: true,
      },
    },
    port: 5173,
  },
});
