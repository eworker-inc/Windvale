import assert from "node:assert/strict";
import {
    Createˉworkbenchˉshell,
    Parseˉcommandˉline,
} from "../Windvale.Playground/wwwroot/js/workbench-shell.js";
import { Validateˉworkspaceˉname } from "../Windvale.Playground/wwwroot/js/workbench-workspace.js";
import {
    Chooseˉworkspaceˉcopyˉname,
    Selectˉrepositoryˉcopy,
} from "../Windvale.Playground/wwwroot/js/workbench-repository-copy.js";

assert.deepEqual(
    Parseˉcommandˉline('write Notes.txt "hello browser workspace"'),
    ["write", "Notes.txt", "hello browser workspace"],
);
assert.deepEqual(Parseˉcommandˉline("run 'Hello-Windvale.wv'"), ["run", "Hello-Windvale.wv"]);
assert.deepEqual(Parseˉcommandˉline("write Empty.txt \"\""), ["write", "Empty.txt", ""]);
assert.throws(() => Parseˉcommandˉline("cat 'unfinished"), /not closed/u);
assert.equal(Validateˉworkspaceˉname("Hello-Windvale.wv"), "Hello-Windvale.wv");
assert.throws(() => Validateˉworkspaceˉname("../outside.wv"), /one ASCII segment/u);
assert.throws(() => Validateˉworkspaceˉname("Nested/file.wv"), /one ASCII segment/u);

const Repositoryˉmanifest = {
    version: 1,
    commit: "a".repeat(40),
    files: [{
        path: "Examples/Seed/Hello-Windvale.wv",
        size: 194,
        contentHash: "b".repeat(64),
        language: "windvale",
        kind: "text",
        publishedUrl: `/repository/code/${"c".repeat(64)}.html`,
    }],
};
const Repositoryˉcopy = Selectˉrepositoryˉcopy(
    Repositoryˉmanifest,
    "Examples/Seed/Hello-Windvale.wv",
);
assert.equal(Repositoryˉcopy.Fileˉname, "Hello-Windvale.wv");
assert.throws(
    () => Selectˉrepositoryˉcopy(Repositoryˉmanifest, "../Hello-Windvale.wv"),
    /malformed/u,
);
assert.throws(
    () => Selectˉrepositoryˉcopy(Repositoryˉmanifest, "README.md"),
    /Windvale source path/u,
);
assert.equal(Chooseˉworkspaceˉcopyˉname("Hello-Windvale.wv", []), "Hello-Windvale.wv");
assert.equal(
    Chooseˉworkspaceˉcopyˉname("Hello-Windvale.wv", [{ Name: "Hello-Windvale.wv" }]),
    "Hello-Windvale-Repository-Copy.wv",
);

const Files = new Map([["Hello.wv", "source one"]]);
const Opened = [];
const Runs = [];
const Workspace = {
    Persistence: "test-memory",
    async List() {
        return Array.from(Files, ([Name, Value]) => ({
            Name,
            Bytes: new TextEncoder().encode(Value).byteLength,
        })).sort((Left, Right) => Left.Name.localeCompare(Right.Name));
    },
    async Readˉtext(Name) {
        if (!Files.has(Name)) {
            throw new Error(`Workspace file not found: ${Name}`);
        }
        return Files.get(Name);
    },
    async Writeˉtext(Name, Value) {
        Files.set(Name, Value);
        return { Name, Bytes: new TextEncoder().encode(Value).byteLength };
    },
    async Delete(Name) {
        if (!Files.delete(Name)) {
            throw new Error(`Workspace file not found: ${Name}`);
        }
    },
};
const Shell = Createˉworkbenchˉshell({
    Workspace,
    Readˉactiveˉsource: () => ({ Name: "Active.wv", Source: "active source" }),
    Openˉsource: (Name, Source) => Opened.push({ Name, Source }),
    Runˉsource: async (Name, Source) => {
        Runs.push({ Name, Source });
        return {
            Standardˉoutput: "Hello from the test\n",
            Executionˉstatus: 0,
            Executionˉresult: 0,
            Wvbˉbytes: 253,
            Wvbˉsha256: "abc123",
            Elapsedˉseconds: 1.25,
        };
    },
});

assert.match((await Shell.Execute("help")).Lines.join("\n"), /run \[file\]/u);
assert.deepEqual((await Shell.Execute("pwd")).Lines, ["/workspace"]);
assert.match((await Shell.Execute("ls")).Lines[0], /^Hello\.wv\s+10 B$/u);
assert.deepEqual((await Shell.Execute("cat Hello.wv")).Lines, ["source one"]);
assert.match((await Shell.Execute("save Saved.wv")).Lines[0], /^saved Saved\.wv/u);
assert.equal(Files.get("Saved.wv"), "active source");
assert.match((await Shell.Execute('write Note.txt "two words"')).Lines[0], /^wrote Note\.txt/u);
assert.equal(Files.get("Note.txt"), "two words\n");
await Shell.Execute("open Saved.wv");
assert.deepEqual(Opened, [{ Name: "Saved.wv", Source: "active source" }]);
const Runˉlines = (await Shell.Execute("run Hello.wv")).Lines;
assert.equal(Runs[0].Name, "Hello.wv");
assert.match(Runˉlines.join("\n"), /Hello from the test/u);
assert.match(Runˉlines.join("\n"), /253 WVB bytes/u);
assert.match((await Shell.Execute("status")).Lines.join("\n"), /test-memory/u);
assert.equal((await Shell.Execute("clear")).Clear, true);
await Shell.Execute("rm Note.txt");
assert.equal(Files.has("Note.txt"), false);
await assert.rejects(() => Shell.Execute("unknown"), /Unknown command/u);
await assert.rejects(() => Shell.Execute("cat"), /Invalid arguments/u);

console.log("Browser-native Windvale Workbench verification passed.");
