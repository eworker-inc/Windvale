import { fileURLToPath } from "node:url";
import { defineConfig } from "vite";
import { onRequestGet as Getˉsupporters } from "./functions/api/supporters.js";

function Windvaleˉsupportersˉdevelopmentˉapi() {
    return {
        name: "windvale-supporters-development-api",
        configureServer(Server) {
            Server.middlewares.use(async (Request, Response, Next) => {
                const Requestˉurl = new URL(Request.url ?? "/", "http://127.0.0.1");
                if (Request.method !== "GET" || Requestˉurl.pathname !== "/api/supporters") {
                    Next();
                    return;
                }

                const Functionˉresponse = await Getˉsupporters({
                    env: {
                        WINDVALE_SUPPORTERS: {
                            get: async () => null,
                        },
                    },
                });

                Response.statusCode = Functionˉresponse.status;
                Functionˉresponse.headers.forEach((Value, Name) => Response.setHeader(Name, Value));
                Response.end(await Functionˉresponse.text());
            });
        },
    };
}

export default defineConfig({
    root: "Website",
    plugins: [Windvaleˉsupportersˉdevelopmentˉapi()],
    build: {
        rollupOptions: {
            input: {
                home: fileURLToPath(new URL("./Website/index.html", import.meta.url)),
                support: fileURLToPath(new URL("./Website/support/index.html", import.meta.url)),
            },
        },
    },
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
