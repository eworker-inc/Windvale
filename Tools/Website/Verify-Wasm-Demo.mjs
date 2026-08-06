import { readdir, readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Demoˉdirectory = path.join(
    Repositoryˉroot,
    "Tools/Windvale.Playground/wwwroot/wasm-demo",
);

const Publicˉsources = await Promise.all([
    "README.md",
    "Website/_headers",
    "Website/sitemap.xml",
    "Tools/Windvale.Playground/README.md",
    "Tools/Windvale.Playground/wwwroot/index.html",
    "Tools/Windvale.Playground/wwwroot/webassembly-compiler/index.html",
].map(Relativeˉpath => readFile(path.join(Repositoryˉroot, Relativeˉpath), "utf8")));

if (Publicˉsources.some(Source => Source.toLowerCase().includes("wasm-demo"))) {
    throw new Error("A public website source still links to the retired direct Wasm demo.");
}

let Demoˉentries;
try {
    Demoˉentries = await readdir(Demoˉdirectory);
} catch (Failure) {
    if (Failure?.code !== "ENOENT") {
        throw Failure;
    }
    Demoˉentries = [];
}
if (Demoˉentries.length !== 0) {
    throw new Error("The retired direct Wasm demo still contains public files.");
}

console.log("Retired direct Wasm demo route verification passed.");
