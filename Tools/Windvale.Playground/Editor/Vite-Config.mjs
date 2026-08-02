import { fileURLToPath } from "node:url";
import { defineConfig } from "vite";

const PROJECT_ROOT = fileURLToPath(new URL("../", import.meta.url));
const OUTPUT_DIRECTORY = fileURLToPath(new URL("../wwwroot/editor/", import.meta.url));
const EDITOR_ENTRY = fileURLToPath(new URL("Playground-Editor.js", import.meta.url));

export default defineConfig({
    root: PROJECT_ROOT,
    base: "./",
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
