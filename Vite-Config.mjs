import { defineConfig } from "vite";

export default defineConfig({
    root: "Website",
    server: {
        host: "127.0.0.1",
        port: 5173,
        strictPort: true,
        proxy: {
            "/playground": {
                target: "http://127.0.0.1:5174",
                changeOrigin: true,
                ws: true,
                rewrite: Path => Path.replace(/^\/playground/, "") || "/",
            },
        },
    },
});
