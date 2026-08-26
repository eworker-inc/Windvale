import assert from "node:assert/strict";
import { compile as Compileˉmonarch } from "../Windvale.Playground/node_modules/monaco-editor/esm/vs/editor/standalone/common/monarch/monarchCompile.js";
import {
    Createˉworkbenchˉshell,
    Parseˉcommandˉline,
} from "../Windvale.Playground/wwwroot/js/workbench-shell.js";
import { Validateˉworkspaceˉname } from "../Windvale.Playground/wwwroot/js/workbench-workspace.js";
import {
    Chooseˉworkspaceˉcopyˉname,
    Selectˉrepositoryˉcopy,
} from "../Windvale.Playground/wwwroot/js/workbench-repository-copy.js";
import {
    Createˉwindvaleˉtokensˉprovider,
    Isˉwindvaleˉnumber,
    WINDVALE_COMPLETIONS,
    WINDVALE_IDENTIFIER_PATTERN,
    WINDVALE_LANGUAGE_1_RESERVED_WORDS,
    WINDVALE_OPERATORS,
} from "../Windvale.Playground/Editor/Windvale-Language.js";

assert.equal(WINDVALE_LANGUAGE_1_RESERVED_WORDS.length, 76);
const Windvaleˉprovider = Createˉwindvaleˉtokensˉprovider();
const Compiledˉwindvaleˉprovider = Compileˉmonarch("windvale", Windvaleˉprovider);

function Readˉfirstˉtoken(Source) {
    for (const Rule of Compiledˉwindvaleˉprovider.tokenizer.root) {
        const Match = Rule.regex.exec(Source);
        if (!Match || Match[0].length === 0) {
            continue;
        }

        const Action = typeof Rule.action === "object" && typeof Rule.action.test === "function"
            ? Rule.action.test(Match[0], Match, "root", Match[0].length === Source.length)
            : Rule.action;
        return {
            Text: Match[0],
            Token: typeof Action === "string" ? Action : Action?.token,
        };
    }

    return undefined;
}

assert.equal(Compiledˉwindvaleˉprovider.unicode, true,
    "The Monaco tokenizer must preserve Unicode property escapes.");
for (const [Word, Expectedˉtoken] of [
    ["module", "keyword.declaration"],
    ["task", "keyword.control"],
    ["scope", "keyword.control"],
    ["policy", "keyword.control"],
    ["cancel_join", "keyword.control"],
    ["await", "keyword.control"],
    ["borrow", "keyword.storage"],
    ["mut", "keyword.storage"],
]) {
    assert.deepEqual(Readˉfirstˉtoken(Word), { Text: Word, Token: Expectedˉtoken },
        `Monaco does not classify ${Word} as ${Expectedˉtoken}.`);
}
assert.deepEqual(
    Readˉfirstˉtoken("Δοκιμήˉτιμή"),
    { Text: "Δοκιμήˉτιμή", Token: "identifier" },
    "Monaco does not preserve one complete Unicode Windvale identifier.",
);
for (const [Source, Opening] of [
    ["\"text\"", "\""],
    ["b\"bytes\"", "b\""],
    ["\"\"\"multiline\"\"\"", "\"\"\""],
    ["r#\"raw\"#", "r#\""],
]) {
    assert.deepEqual(Readˉfirstˉtoken(Source), { Text: Opening, Token: "string.quote" },
        "Monaco does not recognize the opening delimiter in " + Source + ".");
}
const Highlightedˉwords = new Set([
    ...Windvaleˉprovider.declarationKeywords,
    ...Windvaleˉprovider.controlKeywords,
    ...Windvaleˉprovider.storageKeywords,
    ...Windvaleˉprovider.profileKeywords,
    ...Windvaleˉprovider.typeKeywords,
    ...Windvaleˉprovider.literalKeywords,
]);
for (const Word of WINDVALE_LANGUAGE_1_RESERVED_WORDS) {
    assert.equal(Highlightedˉwords.has(Word), true, `Windvale 1.0 word is not highlighted: ${Word}`);
    assert.equal(WINDVALE_COMPLETIONS.some(Completion => Completion.label === Word), true,
        `Windvale 1.0 word is not completed: ${Word}`);
}
assert.equal(Highlightedˉwords.has("while"), true, "The grammar's while terminal must be highlighted.");
assert.deepEqual(WINDVALE_OPERATORS.slice(0, 15), [
    "->", "&&", "||", "<<", ">>", "==", "!=", "<=", ">=", "+=", "-=", "*=", "/=", "%=", "+",
]);
for (const Number of [
    "0", "1_000_000i32", "0xDEAD_BEEFu64", "0b1010_0101u8",
    "1.25", "1e10f32", "0x1.8p+1f64",
]) {
    assert.equal(Isˉwindvaleˉnumber(Number), true, `Windvale 1.0 number is not recognized: ${Number}`);
}
for (const Invalid of ["_1", "1_0_", "0X1u8", "0b102", "1.f32", "0x1.0f64"]) {
    assert.equal(Isˉwindvaleˉnumber(Invalid), false, `Malformed Windvale number is recognized: ${Invalid}`);
}
for (const Identifier of ["Moduleˉreader", "Δοκιμήˉτιμή", "値ˉをˉ読む"]) {
    const Match = WINDVALE_IDENTIFIER_PATTERN.exec(Identifier);
    assert.equal(Match?.index, 0, `Windvale 1.0 identifier does not start-match: ${Identifier}`);
    assert.equal(Match?.[0].length, Identifier.length, `Windvale 1.0 identifier is only partly matched: ${Identifier}`);
}
for (const Invalid of ["ˉModule", "Moduleˉ", "Moduleˉˉreader"]) {
    const Match = WINDVALE_IDENTIFIER_PATTERN.exec(Invalid);
    assert.notEqual(Match?.index === 0 && Match?.[0].length === Invalid.length, true,
        `Malformed macron-separated identifier is recognized: ${Invalid}`);
}
for (let Hashˉcount = 0; Hashˉcount <= 8; Hashˉcount += 1) {
    assert.ok(Windvaleˉprovider.tokenizer[`rawText${Hashˉcount}`]);
    assert.ok(Windvaleˉprovider.tokenizer[`rawBytes${Hashˉcount}`]);
}

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
