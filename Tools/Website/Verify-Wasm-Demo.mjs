import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Demoˉdirectory = path.join(
    Repositoryˉroot,
    "Tools/Windvale.Playground/wwwroot/wasm-demo");
const Hostˉpath = path.join(
    Repositoryˉroot,
    "Tools/Windvale.Playground/wwwroot/js/windvale-wasm-host.js");
const Workerˉpath = path.join(
    Repositoryˉroot,
    "Tools/Windvale.Playground/wwwroot/js/windvale-wasm-worker.js");

const [Index, Application, Host, Worker, Artifact] = await Promise.all([
    readFile(path.join(Demoˉdirectory, "index.html"), "utf8"),
    readFile(path.join(Demoˉdirectory, "app.js"), "utf8"),
    readFile(Hostˉpath, "utf8"),
    readFile(Workerˉpath, "utf8"),
    import(pathToFileURL(path.join(Demoˉdirectory, "windvale-artifact.js"))),
]);

const Requestedˉpaths = Array.from(
    Index.matchAll(/(?:href|src)="([^"]+)"/g),
    Match => Match[1]);
if (Requestedˉpaths.some(Path =>
    Path.includes("_framework") ||
    Path.toLowerCase().includes("blazor") ||
    Path.toLowerCase().includes("dotnet"))) {
    Fail("The standalone page requests a .NET or Blazor framework asset.");
}
Requireˉtext(Index, "./app.js", "standalone application entry");
Requireˉtext(Index, "Three functions · depth 3", "profile-6 call evidence");
Requireˉtext(Application, "../js/windvale-wasm-host.js", "shared disposable worker host");
Requireˉtext(Application, "Countˉframeworkˉrequests", "runtime framework-request assertion");
Requireˉtext(Host, "Workerˉinstance.terminate()", "worker termination boundary");
Requireˉtext(Worker, "WebAssembly.validate", "WebAssembly validation boundary");
Requireˉtext(Worker, "WebAssembly.Module.imports(Module).length !== 0", "import rejection boundary");
Requireˉtext(Worker, "Executeˉabiˉtwo", "metered execution ABI boundary");

const Bytes = Buffer.from(Artifact.ARTIFACT_BASE64, "base64");
Equal(Artifact.ARTIFACT_SIZE, Bytes.byteLength, "artifact byte length");
Equal(
    Artifact.ARTIFACT_SHA256,
    createHash("sha256").update(Bytes).digest("hex"),
    "artifact SHA-256");
if (!WebAssembly.validate(Bytes)) {
    Fail("The embedded artifact does not validate as WebAssembly.");
}

const Module = new WebAssembly.Module(Bytes);
Equal(0, WebAssembly.Module.imports(Module).length, "artifact import count");
Equal(
    JSON.stringify([
        ["Windvale.run", "function"],
        ["Windvale.abi", "global"],
        ["Windvale.result", "global"],
        ["Windvale.instructions", "global"],
    ]),
    JSON.stringify(WebAssembly.Module.exports(Module).map(Item => [Item.name, Item.kind])),
    "artifact export contract");

const Instance = new WebAssembly.Instance(Module, {});
Equal(Artifact.EXPECTED_ABI, Instance.exports["Windvale.abi"].value, "execution ABI");
for (let Run = 1; Run <= 2; Run++) {
    Equal(
        Artifact.SUCCESS_STATUS,
        Instance.exports["Windvale.run"](Artifact.SUCCESS_BUDGET),
        `success run ${Run} status`);
    Equal(Artifact.SUCCESS_RESULT, Instance.exports["Windvale.result"].value, `success run ${Run} result`);
    Equal(
        Artifact.SUCCESS_BUDGET,
        Instance.exports["Windvale.instructions"].value,
        `success run ${Run} instruction count`);
}
Equal(
    Artifact.EXHAUSTION_STATUS,
    Instance.exports["Windvale.run"](Artifact.EXHAUSTION_BUDGET),
    "exhausted run status");
Equal(0, Instance.exports["Windvale.result"].value, "exhausted run result reset");
Equal(
    Artifact.EXHAUSTION_BUDGET,
    Instance.exports["Windvale.instructions"].value,
    "exhausted run instruction count");

console.log(
    "Standalone .NET-free WebAssembly demo verification passed: " +
    `${Bytes.byteLength} bytes, ABI ${Artifact.EXPECTED_ABI}, ` +
    `budget ${Artifact.SUCCESS_BUDGET} result ${Artifact.SUCCESS_RESULT}, ` +
    `budget ${Artifact.EXHAUSTION_BUDGET} status ${Artifact.EXHAUSTION_STATUS}.`);

function Requireˉtext(Value, Expected, Boundary) {
    if (!Value.includes(Expected)) {
        Fail(`The ${Boundary} is missing.`);
    }
}

function Equal(Expected, Actual, Boundary) {
    if (Expected !== Actual) {
        Fail(`Unexpected ${Boundary}: expected ${Expected}, received ${Actual}.`);
    }
}

function Fail(Message) {
    throw new Error(Message);
}
