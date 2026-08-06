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
Requireˉtext(Index, 'src="../analytics.js"', "standalone relative analytics entry");
Requireˉtext(Index, "Main(Input: text)", "profile-8 source provenance");
Requireˉtext(Index, "Fixed 4 MiB input + 4 MiB output", "ABI-3 memory evidence");
Requireˉtext(Application, "../js/windvale-wasm-host.js", "shared disposable worker host");
Requireˉtext(Application, "Countˉframeworkˉrequests", "runtime framework-request assertion");
Requireˉtext(Host, "Workerˉinstance.terminate()", "worker termination boundary");
Requireˉtext(Host, "Input: Transferˉinput.buffer", "worker input transfer boundary");
Requireˉtext(Worker, "WebAssembly.validate", "WebAssembly validation boundary");
Requireˉtext(Worker, "WebAssembly.Module.imports(Module).length !== 0", "import rejection boundary");
Requireˉtext(Worker, "Executeˉabiˉthree", "linear-memory execution ABI boundary");

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
        ["Windvale.memory", "memory"],
        ["Windvale.input_offset", "global"],
        ["Windvale.input_capacity", "global"],
        ["Windvale.output_offset", "global"],
        ["Windvale.output_capacity", "global"],
        ["Windvale.output_length", "global"],
        ["Windvale.output_kind", "global"],
        ["Windvale.instructions", "global"],
    ]),
    JSON.stringify(WebAssembly.Module.exports(Module).map(Item => [Item.name, Item.kind])),
    "artifact export contract");

const Instance = new WebAssembly.Instance(Module, {});
Equal(Artifact.EXPECTED_ABI, Instance.exports["Windvale.abi"].value, "execution ABI");
Equal(
    Artifact.EXPECTED_OUTPUT_KIND,
    Instance.exports["Windvale.output_kind"].value,
    "output kind");
Equal(65_536, Instance.exports["Windvale.input_offset"].value, "input offset");
Equal(4_194_304, Instance.exports["Windvale.input_capacity"].value, "input capacity");
Equal(4_259_840, Instance.exports["Windvale.output_offset"].value, "output offset");
Equal(4_194_304, Instance.exports["Windvale.output_capacity"].value, "output capacity");
Equal(129 * 65_536, Instance.exports["Windvale.memory"].buffer.byteLength, "memory extent");
const Input = new TextEncoder().encode("Hello, 世界 🌬️");
for (let Run = 1; Run <= 2; Run++) {
    new Uint8Array(
        Instance.exports["Windvale.memory"].buffer,
        Instance.exports["Windvale.input_offset"].value,
        Input.byteLength).set(Input);
    Equal(
        Artifact.SUCCESS_STATUS,
        Instance.exports["Windvale.run"](Artifact.SUCCESS_BUDGET, Input.byteLength),
        `success run ${Run} status`);
    Equal(
        Artifact.SUCCESS_BUDGET,
        Instance.exports["Windvale.instructions"].value,
        `success run ${Run} instruction count`);
    Equal(Input.byteLength, Instance.exports["Windvale.output_length"].value, `success run ${Run} output length`);
    const Output = new Uint8Array(
        Instance.exports["Windvale.memory"].buffer,
        Instance.exports["Windvale.output_offset"].value,
        Instance.exports["Windvale.output_length"].value);
    Equal(
        Buffer.from(Input).toString("hex"),
        Buffer.from(Output).toString("hex"),
        `success run ${Run} output bytes`);
}
Equal(
    Artifact.EXHAUSTION_STATUS,
    Instance.exports["Windvale.run"](Artifact.EXHAUSTION_BUDGET, Input.byteLength),
    "exhausted run status");
Equal(0, Instance.exports["Windvale.output_length"].value, "exhausted output reset");
Equal(
    Artifact.EXHAUSTION_BUDGET,
    Instance.exports["Windvale.instructions"].value,
    "exhausted run instruction count");
new Uint8Array(
    Instance.exports["Windvale.memory"].buffer,
    Instance.exports["Windvale.input_offset"].value,
    2).set([0xC0, 0x80]);
Equal(3014, Instance.exports["Windvale.run"](Artifact.SUCCESS_BUDGET, 2), "invalid UTF-8 status");
Equal(0, Instance.exports["Windvale.instructions"].value, "invalid UTF-8 instruction count");
Equal(0, Instance.exports["Windvale.output_length"].value, "invalid UTF-8 output reset");
Equal(
    3008,
    Instance.exports["Windvale.run"](
        Artifact.SUCCESS_BUDGET,
        Instance.exports["Windvale.input_capacity"].value + 1),
    "oversized input status");

console.log(
    "Standalone .NET-free WebAssembly demo verification passed: " +
    `${Bytes.byteLength} bytes, ABI ${Artifact.EXPECTED_ABI}, ` +
    `budget ${Artifact.SUCCESS_BUDGET} UTF-8 bytes ${Input.byteLength}, ` +
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
