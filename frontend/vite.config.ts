import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, ".", "");
  const target = env.WASSERWACHT_API_TARGET || "https://localhost:7001";
  return {
    plugins: [react()],
    server: {
      proxy: {
        "/api": { target, changeOrigin: true, secure: false },
        "/health": { target, changeOrigin: true, secure: false }
      }
    }
  };
});