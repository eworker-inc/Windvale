import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Playgroundˉroot = path.join(Repositoryˉroot, "Tools/Windvale.Playground/wwwroot");
const [
    Playgroundˉindex,
    Playgroundˉapplication,
    Host,
    Worker,
    Core,
    Playgroundˉpackage,
    Websiteˉpackage,
    Deploymentˉworkflow,
    Analyticsˉsource,
    Analyticsˉpublication,
] = await Promise.all([
    readFile(path.join(Playgroundˉroot, "index.html"), "utf8"),
    readFile(path.join(Playgroundˉroot, "js/playground-app.js"), "utf8"),
    readFile(path.join(Playgroundˉroot, "js/windvale-compiler-host.js"), "utf8"),
    readFile(path.join(Playgroundˉroot, "js/windvale-compiler-worker.js"), "utf8"),
    readFile(path.join(Playgroundˉroot, "js/windvale-compiler-core.js"), "utf8"),
    readFile(path.join(Repositoryˉroot, "Tools/Windvale.Playground/package.json"), "utf8"),
    readFile(path.join(Repositoryˉroot, "Website/package.json"), "utf8"),
    readFile(path.join(Repositoryˉroot, ".github/workflows/deploy-homepage.yml"), "utf8"),
    readFile(path.join(Repositoryˉroot, "Website/analytics.js"), "utf8"),
    readFile(path.join(Playgroundˉroot, "analytics.js"), "utf8"),
]);

for (const [Name, Source] of [
    ["playground application", Playgroundˉapplication],
    ["host", Host],
    ["worker", Worker],
    ["core", Core],
]) {
    if (/catch\s*\(\s*Error\s*\)/u.test(Source)) {
        Fail(`The ${Name} shadows the global Error constructor in a catch binding.`);
    }
}

const Requestedˉpaths = Array.from(
    Playgroundˉindex.matchAll(/(?:href|src)="([^"]+)"/g),
    Match => Match[1],
);
if (Requestedˉpaths.some(Path =>
    Path.includes("_framework") ||
    Path.toLowerCase().includes("blazor") ||
    Path.toLowerCase().includes("dotnet"))) {
    Fail("The WebAssembly playground requests a .NET or Blazor framework asset.");
}
if (Analyticsˉpublication !== Analyticsˉsource) {
    Fail("The playground analytics bootstrap is not the shared website source.");
}
Requireˉtext(Playgroundˉindex, 'src="analytics.js"', "relative analytics entry");
Requireˉtext(Playgroundˉindex, "js/playground-app.js", "static playground entry");
Requireˉtext(Playgroundˉindex, "Windvale-native WebAssembly", "native pipeline label");
Requireˉtext(Playgroundˉapplication, "Compileˉandˉrun", "compiler pipeline invocation");
Requireˉtext(Playgroundˉapplication, "Countˉframeworkˉrequests", "framework-request assertion");
Requireˉtext(Playgroundˉapplication, "Editor.Initialize", "Monaco editor integration");
Requireˉtext(Playgroundˉapplication, "New scratch tab unavailable while running", "running source-tab label");
Requireˉtext(Playgroundˉapplication, "Maximum source tabs open", "source-tab ceiling label");
Requireˉtext(Host, "Workerˉinstance.terminate()", "disposable worker boundary");
Requireˉtext(Host, "windvale-compiler-worker.js", "compiler worker entry");
Requireˉtext(Worker, "Manifest.json", "package manifest boundary");
Requireˉtext(Worker, "await Sha256(Bytes)", "artifact identity boundary");
Requireˉtext(Worker, '{ cache: "no-cache" }', "artifact cache revalidation boundary");
Requireˉtext(Worker, "direct-source-compiler-wasm", "direct compiler package entry");
Requireˉtext(Worker, "Compileˉverifyˉexecute", "pipeline invocation");
Requireˉtext(Core, "Buildˉsourceˉset", "canonical WVSS construction");
Requireˉtext(Core, "Readˉwvco", "compiler result validation");
Requireˉtext(Core, "Buildˉscalarˉrequest", "returned-WVB verification and execution");
Requireˉtext(Core, "WebAssembly.Module.imports(Compilerˉmodule).length !== 0", "compiler import rejection");
Requireˉtext(Core, "Memory.grow(1)", "fixed-memory rejection");
Requireˉtext(Core, "2_497 * 65_536", "direct compiler memory boundary");

const Playgroundˉscripts = JSON.parse(Playgroundˉpackage).scripts;
const Websiteˉscripts = JSON.parse(Websiteˉpackage).scripts;
if (!Playgroundˉscripts.dev?.includes("vite wwwroot") ||
    Websiteˉscripts["dev:playground"]?.toLowerCase().includes("dotnet")) {
    Fail("Normal local playground startup is not static and .NET-free.");
}
if (/setup-dotnet|dotnet\s+publish/iu.test(Deploymentˉworkflow)) {
    Fail("Normal website playground publication still invokes .NET.");
}
Requireˉtext(
    Deploymentˉworkflow,
    "Tools/Windvale.Playground/wwwroot/.",
    "static playground publication source",
);

console.log("Static .NET-free direct-compiler playground verification passed.");

function Requireˉtext(Value, Expected, Boundary) {
    if (!Value.includes(Expected)) {
        Fail(`The ${Boundary} is missing.`);
    }
}

function Fail(Message) {
    throw new Error(Message);
}
