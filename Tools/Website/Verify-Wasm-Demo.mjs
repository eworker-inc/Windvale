import { createHash } from "node:crypto";
import { access, readdir, readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Demoˉdirectory = path.join(
    Repositoryˉroot,
    "Tools/Windvale.Playground/wwwroot/wasm-demo",
);
const Previewˉrelativeˉpath = "Website/preview.png";
const Previewˉurl = "https://windvale.ca/preview.png";
const Previewˉsha256 = "47fecbc4a2e0c3c4f0a853715e9ee30377ad95b7b6d6b9f16877645d25d6c460";

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

const Previewˉbytes = await readFile(path.join(Repositoryˉroot, Previewˉrelativeˉpath));
if (createHash("sha256").update(Previewˉbytes).digest("hex") !== Previewˉsha256) {
    throw new Error("The social preview does not match the approved image.");
}
if (Previewˉbytes.readUInt32BE(16) !== 1731 || Previewˉbytes.readUInt32BE(20) !== 909) {
    throw new Error("The social preview dimensions are not 1731 by 909 pixels.");
}

for (const Relativeˉpath of [
    "Website/index.html",
    "Website/progress/index.html",
    "Tools/Windvale.Playground/wwwroot/index.html",
]) {
    const Source = await readFile(path.join(Repositoryˉroot, Relativeˉpath), "utf8");
    for (const Required of [
        Previewˉurl,
        'property="og:image:width" content="1731"',
        'property="og:image:height" content="909"',
        'name="twitter:card" content="summary_large_image"',
    ]) {
        if (!Source.includes(Required)) {
            throw new Error(`${Relativeˉpath} does not publish the approved social preview contract.`);
        }
    }
}

try {
    await access(path.join(Repositoryˉroot, "Website/og.png"));
    throw new Error("The superseded homepage social preview still exists.");
} catch (Failure) {
    if (Failure?.code !== "ENOENT") {
        throw Failure;
    }
}

console.log("Retired direct Wasm demo and social preview verification passed.");
