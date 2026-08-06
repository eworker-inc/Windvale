import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Playgroundˉroot = path.join(Repositoryˉroot, "Tools/Windvale.Playground/wwwroot");
const [Index, Application, Host, Worker, Core, Directˉwasmˉworker] = await Promise.all([
    readFile(path.join(Playgroundˉroot, "webassembly-compiler/index.html"), "utf8"),
    readFile(path.join(Playgroundˉroot, "webassembly-compiler/app.js"), "utf8"),
    readFile(path.join(Playgroundˉroot, "js/windvale-compiler-host.js"), "utf8"),
    readFile(path.join(Playgroundˉroot, "js/windvale-compiler-worker.js"), "utf8"),
    readFile(path.join(Playgroundˉroot, "js/windvale-compiler-core.js"), "utf8"),
    readFile(path.join(Playgroundˉroot, "js/windvale-wasm-worker.js"), "utf8"),
]);

for (const [Name, Source] of [
    ["application", Application],
    ["host", Host],
    ["worker", Worker],
    ["core", Core],
    ["direct WebAssembly worker", Directˉwasmˉworker],
]) {
    if (/catch\s*\(\s*Error\s*\)/u.test(Source)) {
        Fail(`The ${Name} shadows the global Error constructor in a catch binding.`);
    }
}

const Requestedˉpaths = Array.from(
    Index.matchAll(/(?:href|src)="([^"]+)"/g),
    Match => Match[1],
);
if (Requestedˉpaths.some(Path =>
    Path.includes("_framework") ||
    Path.toLowerCase().includes("blazor") ||
    Path.toLowerCase().includes("dotnet"))) {
    Fail("The WebAssembly compiler demo requests a .NET or Blazor framework asset.");
}
Requireˉtext(Index, "./app.js", "static application entry");
Requireˉtext(Index, "Compile · verify · execute", "complete pipeline label");
Requireˉtext(Application, "Countˉframeworkˉrequests", "runtime framework-request assertion");
Requireˉtext(Host, "Workerˉinstance.terminate()", "disposable worker boundary");
Requireˉtext(Host, "windvale-compiler-worker.js", "compiler worker entry");
Requireˉtext(Worker, "Manifest.json", "package manifest boundary");
Requireˉtext(Worker, "await Sha256(Bytes)", "artifact identity boundary");
Requireˉtext(Worker, "Compileˉverifyˉexecute", "pipeline invocation");
Requireˉtext(Core, "Buildˉsourceˉset", "canonical WVSS construction");
Requireˉtext(Core, "Readˉwvco", "compiler result validation");
Requireˉtext(Core, "Buildˉscalarˉrequest", "returned-WVB verification and execution");
Requireˉtext(Core, "WebAssembly.Module.imports(Module).length !== 0", "import rejection");
Requireˉtext(Core, "Memory.grow(1)", "fixed-memory rejection");

console.log("Static .NET-free WebAssembly compiler demo verification passed.");

function Requireˉtext(Value, Expected, Boundary) {
    if (!Value.includes(Expected)) {
        Fail(`The ${Boundary} is missing.`);
    }
}

function Fail(Message) {
    throw new Error(Message);
}
