import { copyFile, mkdir } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { defineConfig } from "vite";
import { onRequestGet as Getˉsupporters } from "./functions/api/supporters.js";

const PROJECT_ROOT = fileURLToPath(new URL("./", import.meta.url));
const STATIC_FILES = Object.freeze([
    "_headers",
    "robots.txt",
    "sitemap.xml",
    "preview.png",
    "preview-2026-08-06.jpg",
    "preview-square-2026-08-06.png",
    "support-og.png",
    "windvale-logo.png",
    "assets/nova-scotia-coast.webp",
    "assets/material-symbols-LICENSE.txt",
]);

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

function Windvaleˉstaticˉpublicationˉfiles() {
    let Outputˉdirectory;
    return {
        name: "windvale-static-publication-files",
        configResolved(Config) {
            Outputˉdirectory = Config.build.outDir;
        },
        async closeBundle() {
            await Promise.all(STATIC_FILES.map(async (Relativeˉpath) => {
                const Targetˉpath = path.join(Outputˉdirectory, ...Relativeˉpath.split("/"));
                await mkdir(path.dirname(Targetˉpath), { recursive: true });
                await copyFile(path.join(PROJECT_ROOT, ...Relativeˉpath.split("/")), Targetˉpath);
            }));
        },
    };
}

export default defineConfig({
    root: PROJECT_ROOT,
    publicDir: "Generated",
    plugins: [
        Windvaleˉsupportersˉdevelopmentˉapi(),
        Windvaleˉstaticˉpublicationˉfiles(),
    ],
    build: {
        rollupOptions: {
            input: {
                home: fileURLToPath(new URL("./index.html", import.meta.url)),
                notFound: fileURLToPath(new URL("./404.html", import.meta.url)),
                support: fileURLToPath(new URL("./support/index.html", import.meta.url)),
                progress: fileURLToPath(new URL("./progress/index.html", import.meta.url)),
                documents: fileURLToPath(new URL("./docs/index.html", import.meta.url)),
                code: fileURLToPath(new URL("./code/index.html", import.meta.url)),
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
            },
        },
    },
});
