import { copyFile, mkdir } from "node:fs/promises";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { defineConfig } from "vite";

const PROJECT_ROOT = fileURLToPath(new URL("../", import.meta.url));
const OUTPUT_DIRECTORY = fileURLToPath(new URL("../wwwroot/editor/", import.meta.url));
const EDITOR_ENTRY = fileURLToPath(new URL("Playground-Editor.js", import.meta.url));
const THIRD_PARTY_NOTICES = Object.freeze([
    ["monaco-editor/LICENSE", "monaco-editor-LICENSE.txt"],
    ["monaco-editor/ThirdPartyNotices.txt", "monaco-editor-ThirdPartyNotices.txt"],
    ["dompurify/LICENSE", "DOMPurify-LICENSE.txt"],
    ["marked/LICENSE.md", "marked-LICENSE.md"],
]);

export default defineConfig({
    root: PROJECT_ROOT,
    base: "./",
    plugins: [
        {
            name: "windvale-editor-third-party-notices",
            async closeBundle() {
                const Noticeˉdirectory = join(OUTPUT_DIRECTORY, "notices");
                await mkdir(Noticeˉdirectory, { recursive: true });
                await Promise.all(THIRD_PARTY_NOTICES.map(async ([Source, Target]) => {
                    await copyFile(
                        join(PROJECT_ROOT, "node_modules", ...Source.split("/")),
                        join(Noticeˉdirectory, Target));
                }));
            },
        },
    ],
    build: {
        outDir: OUTPUT_DIRECTORY,
        emptyOutDir: true,
        sourcemap: false,
        lib: {
            entry: EDITOR_ENTRY,
            formats: ["es"],
            fileName: "playground-editor",
            cssFileName: "playground-editor",
        },
        rollupOptions: {
            output: {
                entryFileNames: "playground-editor.js",
                chunkFileNames: "assets/[name]-[hash].js",
            },
        },
    },
});
