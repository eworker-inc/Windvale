import { createHash } from "node:crypto";
import {
    existsSync,
    lstatSync,
    readFileSync,
    readdirSync,
} from "node:fs";
import { dirname, join, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { TextDecoder } from "node:util";

const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, "..", "..");
const FREEZE_PATH = "Documents/Project/Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt";
const INVENTORY_PATH = "Tests/Native/Language-1.0-Fixture-Inventory.txt";
const FREEZE_BYTES = 3702;
const FREEZE_SHA256 = "c9517841eae6b6e86778cb1dd88711feb38929dec8fe79e084eec44fa22c512a";
const COMPLETE_COUNT = 250;
const COMPLETE_BYTES = 1724854;
const COMPLETE_STREAM_BYTES = 46043;
const COMPLETE_SHA256 = "fb918a763ae7c8c85dd1a2ffecee6587ab93bbf846ae31ae19b53509aed36a0a";

function Fail(message) {
    throw new Error(message);
}

function Hash(bytes) {
    return createHash("sha256").update(bytes).digest("hex");
}

function Repositoryˉpath(path) {
    const absolute = resolve(REPOSITORY_ROOT, ...path.split("/"));
    const prefix = REPOSITORY_ROOT.endsWith(sep) ? REPOSITORY_ROOT : REPOSITORY_ROOT + sep;
    if (absolute !== REPOSITORY_ROOT && !absolute.startsWith(prefix)) {
        Fail(`Path escapes repository: ${path}`);
    }
    return absolute;
}

function Ordinaryˉfile(path) {
    const absolute = Repositoryˉpath(path);
    const stat = lstatSync(absolute);
    if (!stat.isFile() || stat.isSymbolicLink()) {
        Fail(`Expected ordinary non-link file: ${path}`);
    }
    const bytes = readFileSync(absolute);
    return { path, bytes: bytes.length, sha256: Hash(bytes), content: bytes };
}

function Recursiveˉfiles(root, predicate = () => true) {
    const result = [];
    function Visit(absolute) {
        for (const name of readdirSync(absolute).sort()) {
            const child = join(absolute, name);
            const stat = lstatSync(child);
            if (stat.isSymbolicLink()) {
                Fail(`Symbolic link is outside the frozen input model: ${child}`);
            }
            if (stat.isDirectory()) {
                Visit(child);
            } else if (stat.isFile()) {
                const path = relative(REPOSITORY_ROOT, child).split(sep).join("/");
                if (predicate(path)) result.push(Ordinaryˉfile(path));
            } else {
                Fail(`Non-regular frozen input: ${child}`);
            }
        }
    }
    Visit(Repositoryˉpath(root));
    return result.sort((left, right) => left.path < right.path ? -1 : left.path > right.path ? 1 : 0);
}

function Entryˉstream(entries) {
    return Buffer.from(entries.map((entry) =>
        `path=${entry.path}\nbytes=${entry.bytes}\nsha256=${entry.sha256}\n`
    ).join(""), "utf8");
}

function Checkˉset(name, entries, count, bytes, streamBytes, sha256) {
    const stream = Entryˉstream(entries);
    const total = entries.reduce((sum, entry) => sum + entry.bytes, 0);
    if (entries.length !== count || total !== bytes || stream.length !== streamBytes || Hash(stream) !== sha256) {
        Fail(`${name} frozen identity differs`);
    }
}

function Checkˉdescriptor(entry) {
    if (entry.content.length >= 3 && entry.content[0] === 0xef &&
        entry.content[1] === 0xbb && entry.content[2] === 0xbf) {
        Fail(`Source has a byte-order mark: ${entry.path}`);
    }
    new TextDecoder("utf-8", { fatal: true }).decode(entry.content);
    const lf = entry.content.indexOf(0x0a);
    if (lf < 0) Fail(`Source descriptor has no logical line ending: ${entry.path}`);
    const crlf = lf > 0 && entry.content[lf - 1] === 0x0d;
    const length = crlf ? lf - 1 : lf;
    if (length > 128) Fail(`Source descriptor exceeds 128 bytes: ${entry.path}`);
    const descriptor = entry.content.subarray(0, length);
    for (const value of descriptor) {
        if (value > 0x7f) Fail(`Source descriptor is not ASCII: ${entry.path}`);
    }
    const text = descriptor.toString("ascii");
    const match = /^#!wv\/1 ([A-Za-z][A-Za-z0-9]*(?:-[A-Za-z][A-Za-z0-9]*)*(?:\.[A-Za-z][A-Za-z0-9]*(?:-[A-Za-z][A-Za-z0-9]*)*)*)@([1-9][0-9]*)$/.exec(text);
    if (!match) Fail(`Source descriptor grammar differs: ${entry.path}`);
    if (Buffer.byteLength(match[1], "ascii") < 2 || Buffer.byteLength(match[1], "ascii") > 96) {
        Fail(`Source profile length differs: ${entry.path}`);
    }
    if (BigInt(match[2]) > 4294967295n) Fail(`Source profile version exceeds u32: ${entry.path}`);
}

const freeze = Ordinaryˉfile(FREEZE_PATH);
if (freeze.bytes !== FREEZE_BYTES || freeze.sha256 !== FREEZE_SHA256) {
    Fail("Replacement source-freeze manifest identity differs");
}
const freezeText = freeze.content.toString("utf8");
if (freezeText.includes("\r")) Fail("Replacement source-freeze manifest is not LF-only");
const core = [];
let section = "";
for (const line of freezeText.split("\n")) {
    const heading = /^\[([^\]]+)\]$/.exec(line);
    if (heading) {
        section = heading[1];
        continue;
    }
    if (section !== "core") continue;
    const match = /^([0-9a-f]{64}) ([0-9]+) (.+)$/.exec(line);
    if (!match) continue;
    const entry = Ordinaryˉfile(match[3]);
    if (entry.bytes !== Number(match[2]) || entry.sha256 !== match[1]) {
        Fail(`Frozen core input differs: ${entry.path}`);
    }
    core.push(entry);
}
if (core.length !== 13) Fail("Frozen core input count differs");

const decisions = readdirSync(Repositoryˉpath("Documents/Decisions"))
    .filter((name) => {
        const match = /^(\d{4})-.*\.md$/.exec(name);
        return match && Number(match[1]) >= 751 && Number(match[1]) <= 766;
    })
    .sort()
    .map((name) => Ordinaryˉfile(`Documents/Decisions/${name}`));
const paper = Recursiveˉfiles("Documents/Project/Language-1.0-Paper-Corpus");
const localization = Recursiveˉfiles("Documents/Project/Language-1.0-Localization-Workloads");
Checkˉset("decisions", decisions, 16, 126070, 2603, "39573331530bceee4481f9758f2f98adc1911b5df58275769ee482d0d0567c0b");
Checkˉset("paper corpus", paper, 158, 980348, 28885, "56a33590b66a51654f0e98e780767ba249e711c742bead7846bb986cb468798e");
Checkˉset("localization corpus", localization, 63, 187507, 12718, "67619c45ddc2f038e8b509a4bcf068e5d0cce1799ee52a90b7dd12f84f1526ac");
const complete = [...core, ...decisions, ...paper, ...localization]
    .sort((left, right) => left.path < right.path ? -1 : left.path > right.path ? 1 : 0);
Checkˉset("complete candidate", complete, COMPLETE_COUNT, COMPLETE_BYTES, COMPLETE_STREAM_BYTES, COMPLETE_SHA256);

const inventory = Ordinaryˉfile(INVENTORY_PATH).content.toString("utf8");
if (inventory.includes("\r")) Fail("Fixture inventory is not LF-only");
const inventoryLines = inventory.trimEnd().split("\n");
if (inventoryLines.shift() !== "windvale-language-1-source-fixture-inventory 1") {
    Fail("Fixture inventory header differs");
}
const freezeLine = inventoryLines.shift()?.split("|");
if (!freezeLine || freezeLine.length !== 4 || freezeLine[0] !== "freeze" ||
    freezeLine[1] !== FREEZE_PATH || Number(freezeLine[2]) !== FREEZE_BYTES ||
    freezeLine[3] !== FREEZE_SHA256) {
    Fail("Fixture inventory freeze binding differs");
}
if (inventoryLines.length !== 16) Fail("Fixture bundle count differs");
let sourceCount = 0;
let sourceBytes = 0;
const seenBundles = new Set();
for (const line of inventoryLines) {
    const fields = line.split("|");
    if (fields.length !== 7 || fields[0] !== "bundle") Fail(`Invalid fixture inventory row: ${line}`);
    const [, bundle, countText, bytesText, expectedHash, slices, standing] = fields;
    if (seenBundles.has(bundle)) Fail(`Duplicate fixture bundle: ${bundle}`);
    seenBundles.add(bundle);
    if (!/^slices?-/.test(slices) || !standing.startsWith("identity-only")) {
        Fail(`Invalid migration standing: ${bundle}`);
    }
    const sourceRoot = `${bundle}/Source`;
    const sources = existsSync(Repositoryˉpath(sourceRoot))
        ? Recursiveˉfiles(sourceRoot, (path) => path.endsWith(".wv"))
        : [];
    const sourceStream = Buffer.from(sources.map((entry) =>
        `${entry.path}|${entry.bytes}|${entry.sha256}\n`
    ).join(""), "utf8");
    const bytes = sources.reduce((sum, entry) => sum + entry.bytes, 0);
    if (sources.length !== Number(countText) || bytes !== Number(bytesText) || Hash(sourceStream) !== expectedHash) {
        Fail(`Fixture bundle identity differs: ${bundle}`);
    }
    for (const source of sources) Checkˉdescriptor(source);
    sourceCount += sources.length;
    sourceBytes += bytes;
}
if (sourceCount !== 72 || sourceBytes !== 482325) Fail("Complete source fixture inventory differs");
