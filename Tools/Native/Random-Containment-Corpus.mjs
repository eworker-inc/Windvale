import { createHash } from "node:crypto";
import { readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { spawnSync } from "node:child_process";

const ARCHIVE_SHA256 =
    "c3d17ee927d8c485fc98b85c4b50d5fb6110532b8a2d02b818d7018903f2edc6";
const MANIFEST_SHA256 =
    "d7076c44f43192db832796553cbe605c20829361d7249e111a270ff22458186c";
const HEADER = "windvale-random-containment-corpus 1";
const COLUMNS =
    "name|family|case|input-units|bytes|sha256|primary-outcome|" +
    "primary-code|primary-offset|secondary-outcome|secondary-code";

export async function Loadˉrandomˉcontainmentˉcorpus(
    Repositoryˉroot,
    Temporaryˉdirectory,
) {
    const Encodedˉpath = path.join(
        Repositoryˉroot,
        "Tests/Native/Random-Containment/Corpus.tar.gz.b64",
    );
    const Encoded = await readFile(Encodedˉpath, "ascii");
    Require(!Encoded.includes("\r"), "The corpus encoding must use LF line endings.");
    Require(Encoded.endsWith("\n"), "The corpus encoding lacks its final LF.");
    Require(
        /^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?\n$/u
            .test(Encoded.replaceAll("\n", "") + "\n"),
        "The corpus encoding is not canonical base64.",
    );
    const Compact = Encoded.replaceAll("\n", "");
    const Archive = Buffer.from(Compact, "base64");
    Require(
        Archive.toString("base64") === Compact,
        "The corpus encoding does not reproduce canonically.",
    );
    Require(Sha256(Archive) === ARCHIVE_SHA256, "The corpus archive identity differs.");

    const Archiveˉpath = path.join(Temporaryˉdirectory, "Corpus.tar.gz");
    await writeFile(Archiveˉpath, Archive, { flag: "wx" });
    const Listing = Runˉtar(["-tzf", Archiveˉpath], "list");
    const Entries = Lines(Listing);
    Require(Entries.length === 2001, "The corpus archive entry count differs.");
    Require(Entries[0] === "Manifest.txt", "The corpus manifest entry differs.");
    for (const Entry of Entries) {
        Require(
            Entry === path.basename(Entry) && !Entry.includes("\\"),
            `The corpus entry '${Entry}' is not a plain filename.`,
        );
    }
    Require(new Set(Entries).size === Entries.length, "The corpus archive repeats an entry.");

    Runˉtar(
        ["-xzf", Archiveˉpath, "-C", Temporaryˉdirectory],
        "extract",
    );
    const Manifestˉpath = path.join(Temporaryˉdirectory, "Manifest.txt");
    const Manifestˉbytes = await readFile(Manifestˉpath);
    Require(
        Sha256(Manifestˉbytes) === MANIFEST_SHA256,
        "The corpus manifest identity differs.",
    );
    const Manifest = Decodeˉutf8(Manifestˉbytes, "manifest");
    Require(Manifest.endsWith("\n"), "The corpus manifest lacks its final LF.");
    const Manifestˉlines = Manifest.slice(0, -1).split("\n");
    Require(Manifestˉlines[0] === HEADER, "The corpus manifest header differs.");
    Require(Manifestˉlines[1] === COLUMNS, "The corpus manifest columns differ.");
    Require(Manifestˉlines.length === 2002, "The corpus manifest row count differs.");

    const Cases = [];
    const Familyˉcounts = { source: 0, wvb: 0, wvo: 0 };
    for (const Line of Manifestˉlines.slice(2)) {
        const Fields = Line.split("|");
        Require(Fields.length === 11, "A corpus manifest row has the wrong field count.");
        const [
            Name,
            Family,
            Numberˉtext,
            Inputˉunitsˉtext,
            Byteˉlengthˉtext,
            Digest,
            Primaryˉoutcome,
            Primaryˉcode,
            Primaryˉoffset,
            Secondaryˉoutcome,
            Secondaryˉcode,
        ] = Fields;
        Require(Object.hasOwn(Familyˉcounts, Family), `Unknown corpus family '${Family}'.`);
        const Number = Parseˉdecimal(Numberˉtext, "case number");
        const Inputˉunits = Parseˉdecimal(Inputˉunitsˉtext, "input-unit count");
        const Byteˉlength = Parseˉdecimal(Byteˉlengthˉtext, "byte length");
        Require(Number === Familyˉcounts[Family], `The ${Family} sequence is not contiguous.`);
        Require(Inputˉunits <= 511, `The ${Name} input-unit bound differs.`);
        Require(Byteˉlength <= 1022, `The ${Name} byte bound differs.`);
        Require(/^[0-9a-f]{64}$/u.test(Digest), `The ${Name} digest is invalid.`);
        Require(Primaryˉoutcome === "rejected", `The ${Name} Stage 0 outcome differs.`);

        if (Family === "source") {
            Require(Name === `Source-${Number.toString().padStart(3, "0")}.wv`,
                `The source filename '${Name}' differs.`);
            Require(/^WVC[0-9]{4}$/u.test(Primaryˉcode), `The ${Name} compiler code differs.`);
            Require(Primaryˉoffset === "-", `The ${Name} compiler offset differs.`);
            Require(Secondaryˉoutcome === "rejected", `The ${Name} assembler outcome differs.`);
            Require(Secondaryˉcode === "WVA1001", `The ${Name} assembler code differs.`);
        } else {
            const Width = Family === "wvb" ? 4 : 3;
            const Extension = Family;
            const Prefix = Family === "wvb" ? "Wvb" : "Wvo";
            Require(Name === `${Prefix}-${Number.toString().padStart(Width, "0")}.${Extension}`,
                `The ${Family} filename '${Name}' differs.`);
            Require(Inputˉunits === Byteˉlength, `The ${Name} unit count differs.`);
            Require(
                new RegExp(`^${Family.toUpperCase()}[0-9]{4}$`, "u").test(Primaryˉcode),
                `The ${Name} Stage 0 code differs.`,
            );
            Require(Primaryˉoffset === "0", `The ${Name} Stage 0 offset differs.`);
            Require(Secondaryˉoutcome === "-" && Secondaryˉcode === "-",
                `The ${Name} secondary oracle fields differ.`);
        }

        const Inputˉpath = path.resolve(Temporaryˉdirectory, Name);
        Require(
            path.dirname(Inputˉpath) === path.resolve(Temporaryˉdirectory),
            `The ${Name} corpus path escapes its directory.`,
        );
        const Bytes = await readFile(Inputˉpath);
        Require(Bytes.byteLength === Byteˉlength, `The ${Name} byte length differs.`);
        Require(Sha256(Bytes) === Digest, `The ${Name} identity differs.`);
        Cases.push({
            Name,
            Family,
            Number,
            Inputˉunits,
            Byteˉlength,
            Digest,
            Primaryˉoutcome,
            Primaryˉcode,
            Primaryˉoffset,
            Secondaryˉoutcome,
            Secondaryˉcode,
            Inputˉpath,
            Bytes,
        });
        Familyˉcounts[Family] += 1;
    }

    Require(Familyˉcounts.source === 500, "The source corpus count differs.");
    Require(Familyˉcounts.wvb === 1000, "The WVB corpus count differs.");
    Require(Familyˉcounts.wvo === 500, "The WVO corpus count differs.");
    Require(
        JSON.stringify(Entries.slice(1)) === JSON.stringify(Cases.map(Item => Item.Name)),
        "The corpus archive and manifest order differ.",
    );
    return Cases;
}

export function Sha256(Bytes) {
    return createHash("sha256").update(Bytes).digest("hex");
}

export function Decodeˉutf8(Bytes, Boundary) {
    try {
        return new TextDecoder("utf-8", { fatal: true }).decode(Bytes);
    } catch {
        throw new Error(`The ${Boundary} is not strict UTF-8.`);
    }
}

function Parseˉdecimal(Text, Boundary) {
    Require(/^(?:0|[1-9][0-9]*)$/u.test(Text), `The ${Boundary} is not canonical decimal.`);
    const Value = Number(Text);
    Require(Number.isSafeInteger(Value), `The ${Boundary} is outside the safe range.`);
    return Value;
}

function Runˉtar(Arguments, Operation) {
    const Result = spawnSync("tar", Arguments, {
        encoding: "utf8",
        windowsHide: true,
    });
    if (Result.error !== undefined) {
        throw Result.error;
    }
    Require(Result.status === 0, `The corpus ${Operation} operation failed.`);
    Require(Result.stderr === "", `The corpus ${Operation} operation wrote a diagnostic.`);
    if (Operation === "extract") {
        Require(Result.stdout === "", "The corpus extract operation wrote output.");
    }
    return Result.stdout;
}

function Lines(Text) {
    const Normalized = Text.replaceAll("\r\n", "\n");
    Require(Normalized.endsWith("\n"), "The archive listing lacks its final line ending.");
    return Normalized.slice(0, -1).split("\n");
}

function Require(Condition, Message) {
    if (!Condition) {
        throw new Error(Message);
    }
}
