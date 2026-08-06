import { createHash } from "node:crypto";
import { copyFile, mkdir, readFile, rm } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Sourceˉroot = path.join(
    Repositoryˉroot,
    "Artifacts/WebAssembly-Playground",
);
const Destinationˉroot = path.join(
    Repositoryˉroot,
    "Tools/Windvale.Playground/wwwroot/webassembly-compiler/artifacts",
);
const Expectedˉdestination = path.join(
    Repositoryˉroot,
    "Tools/Windvale.Playground/wwwroot/webassembly-compiler",
);
if (!Destinationˉroot.startsWith(`${Expectedˉdestination}${path.sep}`)) {
    throw new Error("The WebAssembly playground package destination is invalid.");
}

const Manifestˉbytes = await readFile(path.join(Sourceˉroot, "Manifest.json"));
const Manifest = JSON.parse(Manifestˉbytes.toString("utf8"));
if (Manifest.format !== "windvale-webassembly-playground-1" ||
    !Array.isArray(Manifest.artifacts) ||
    Manifest.artifacts.length !== 3) {
    throw new Error("The WebAssembly playground package manifest is invalid.");
}

const Files = [];
for (const Artifact of Manifest.artifacts) {
    if (typeof Artifact.path !== "string" ||
        path.basename(Artifact.path) !== Artifact.path ||
        !Number.isInteger(Artifact.bytes) ||
        typeof Artifact.sha256 !== "string") {
        throw new Error("A WebAssembly playground artifact entry is invalid.");
    }
    const Bytes = await readFile(path.join(Sourceˉroot, Artifact.path));
    const Digest = createHash("sha256").update(Bytes).digest("hex");
    if (Bytes.byteLength !== Artifact.bytes || Digest !== Artifact.sha256) {
        throw new Error(`The '${Artifact.name}' package identity is invalid.`);
    }
    Files.push(Artifact.path);
}

await rm(Destinationˉroot, { recursive: true, force: true });
await mkdir(Destinationˉroot, { recursive: true });
await Promise.all([
    copyFile(
        path.join(Sourceˉroot, "Manifest.json"),
        path.join(Destinationˉroot, "Manifest.json"),
    ),
    ...Files.map(File => copyFile(
        path.join(Sourceˉroot, File),
        path.join(Destinationˉroot, File),
    )),
]);

console.log(
    `Packaged ${Files.length} verified WebAssembly playground artifacts without .NET.`,
);
