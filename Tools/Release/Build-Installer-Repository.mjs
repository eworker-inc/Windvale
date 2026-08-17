import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { Gzipˉdeflate } from "./Deterministic-Compression.mjs";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, "../..");
const DEFAULT_INPUT = "Distribution/Installers/Windvale-Development-Installer.json";
const SAFE_PATH = /^[A-Za-z0-9._/-]+$/;
const IDENTIFIER = /^[a-z0-9](?:[a-z0-9.-]*[a-z0-9])?$/;
const SHA256 = /^[0-9a-f]{64}$/;
const VERSION = /^0\.[0-9]+\.[0-9]+(?:-dev\.[0-9]+)?$/;
const MAX_INDEX_BYTES = 1024 * 1024;
const MAX_EXPANDED_SELECTION_BYTES = 64 * 1024 * 1024;
const MAX_OBJECT_BYTES = 64 * 1024 * 1024;
const MAX_OBJECTS = 256;
const MAX_COMPONENTS = 64;
const MAX_PROFILES = 16;

function Fail(Message) {
    throw new Error(Message);
}

function Sha256(Value) {
    return crypto.createHash("sha256").update(Value).digest("hex");
}

function Compareˉtext(Left, Right) {
    return Buffer.from(Left, "utf8").compare(Buffer.from(Right, "utf8"));
}

function Assertˉidentifier(Value, Description) {
    if (typeof Value !== "string" || Value.length > 64 || !IDENTIFIER.test(Value)) {
        Fail(`Invalid ${Description}: ${Value}`);
    }
    return Value;
}

function Assertˉpath(Value, Description) {
    if (typeof Value !== "string" || Value.length > 1024 ||
        !SAFE_PATH.test(Value) || Value.startsWith("/") || Value.endsWith("/") ||
        Value.includes("//") || Value.split("/").some(Part => Part === "." || Part === "..")) {
        Fail(`Invalid ${Description}: ${Value}`);
    }
    return Value;
}

function Assertˉmode(Value) {
    if (!/^0[0-7]{3}$/.test(Value ?? "")) Fail(`Invalid repository mode: ${Value}`);
    return Value;
}

function Readˉcanonicalˉtext(FilePath) {
    const Bytes = fs.readFileSync(FilePath);
    const Text = Bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Bytes) || Text.includes("\0")) {
        Fail(`Repository text is not strict UTF-8: ${FilePath}`);
    }
    return Text;
}

function Readˉdeclaredˉfile(Declaration, Target) {
    const Component = Assertˉidentifier(Declaration.component, "component identifier");
    const Source = Assertˉpath(Declaration.source, "source path");
    const Destination = Assertˉpath(Declaration.path, "installation path");
    const SourcePath = path.join(REPOSITORY_ROOT, ...Source.split("/"));
    const Stat = fs.lstatSync(SourcePath);
    if (!Stat.isFile() || Stat.isSymbolicLink()) {
        Fail(`Repository source is not an ordinary file: ${Source}`);
    }
    let Bytes;
    if (Declaration.text === "lf") {
        const Text = Readˉcanonicalˉtext(SourcePath).replaceAll("\r\n", "\n");
        if (Text.includes("\r")) Fail(`Repository source contains bare CR: ${Source}`);
        Bytes = Buffer.from(Text, "utf8");
    } else if (Declaration.text === undefined) {
        Bytes = fs.readFileSync(SourcePath);
    } else {
        Fail(`Unsupported repository text policy: ${Declaration.text}`);
    }
    if (!Number.isSafeInteger(Declaration.bytes) || Declaration.bytes < 0 ||
        Declaration.bytes > MAX_OBJECT_BYTES || Bytes.length !== Declaration.bytes ||
        !SHA256.test(Declaration.sha256 ?? "") || Sha256(Bytes) !== Declaration.sha256) {
        Fail(`Repository input identity changed: ${Source}`);
    }
    const Packed = Gzipˉdeflate(Bytes);
    return {
        component: Component,
        target: Target,
        path: Destination,
        mode: Assertˉmode(Declaration.mode),
        sha256: Declaration.sha256,
        bytes: Bytes.length,
        encoding: "gzip-1",
        blobSha256: Sha256(Packed),
        blobBytes: Packed.length,
        packed: Packed,
    };
}

function Resolveˉinput(Value) {
    const Relative = Assertˉpath(Value ?? DEFAULT_INPUT, "repository input path");
    if (!Relative.startsWith("Distribution/Installers/") || !Relative.endsWith(".json")) {
        Fail("The repository input must be checked-in installer metadata.");
    }
    return path.join(REPOSITORY_ROOT, ...Relative.split("/"));
}

function Assertˉemptyˉoutput(OutputPath) {
    const Stat = fs.lstatSync(OutputPath);
    if (!Stat.isDirectory() || Stat.isSymbolicLink() || fs.readdirSync(OutputPath).length) {
        Fail("The repository output must be an existing empty ordinary directory.");
    }
    return fs.realpathSync(OutputPath);
}

function Assertˉorderedˉunique(Values, Description) {
    for (let Index = 0; Index < Values.length; Index++) {
        if (Index > 0 && Compareˉtext(Values[Index - 1], Values[Index]) >= 0) {
            Fail(`${Description} must be bytewise ordered and unique.`);
        }
    }
}

function Loadˉinput(InputPath) {
    const Input = JSON.parse(Readˉcanonicalˉtext(Resolveˉinput(InputPath)));
    if (Input.format !== "windvale-installer-input-1" ||
        !VERSION.test(Input.version ?? "") ||
        !["development", "stable"].includes(Input.channel) ||
        Input.repository?.format !== "windvale-installer-repository-input-1" ||
        !Array.isArray(Input.sharedFiles) || !Array.isArray(Input.targets) ||
        !Array.isArray(Input.repository.profiles) ||
        Input.repository.profiles.length < 1 ||
        Input.repository.profiles.length > MAX_PROFILES) {
        Fail("The installer repository input envelope is invalid.");
    }
    const TargetIds = Input.targets.map(Target => Target.id);
    if (TargetIds.join("|") !== "windows-x64|linux-x64" ||
        Input.targets.some(Target => !Array.isArray(Target.files))) {
        Fail("The installer repository target set is invalid.");
    }
    const Profiles = Input.repository.profiles.map(Profile => {
        const Id = Assertˉidentifier(Profile.id, "profile identifier");
        if (!Array.isArray(Profile.components) || Profile.components.length < 1 ||
            Profile.components.length > MAX_COMPONENTS) {
            Fail(`Invalid repository profile: ${Id}`);
        }
        const Components = Profile.components.map(Component =>
            Assertˉidentifier(Component, "profile component"));
        Assertˉorderedˉunique(Components, `Profile ${Id} components`);
        return { id: Id, components: Components };
    });
    Assertˉorderedˉunique(Profiles.map(Profile => Profile.id), "Repository profiles");
    return { input: Input, profiles: Profiles };
}

function Selectionˉentries(Entries, Profile, Target) {
    const Components = new Set(Profile.components);
    const Selected = Entries.filter(Entry =>
        Components.has(Entry.component) && (Entry.target === "all" || Entry.target === Target));
    for (const Component of Profile.components) {
        if (!Selected.some(Entry => Entry.component === Component)) {
            Fail(`Profile ${Profile.id} has no ${Target} object for ${Component}.`);
        }
    }
    const Paths = new Set();
    let ExpandedBytes = 0;
    for (const Entry of Selected) {
        if (Paths.has(Entry.path)) Fail(`Profile ${Profile.id} repeats path ${Entry.path}.`);
        Paths.add(Entry.path);
        ExpandedBytes += Entry.bytes;
    }
    if (ExpandedBytes > MAX_EXPANDED_SELECTION_BYTES) {
        Fail(`Profile ${Profile.id} exceeds the expanded-byte bound for ${Target}.`);
    }
    return Selected;
}

function Buildˉrepository(OutputPath, InputPath) {
    process.stdout.write("installer repository step=load-input item=1/4\n");
    const { input: Input, profiles: Profiles } = Loadˉinput(InputPath);
    const Output = Assertˉemptyˉoutput(OutputPath);
    const Declarations = [
        ...Input.sharedFiles.map(Declaration => ({ declaration: Declaration, target: "all" })),
        ...Input.targets.flatMap(Target => Target.files.map(Declaration => ({
            declaration: Declaration,
            target: Target.id,
        }))),
    ];
    if (Declarations.length < 1 || Declarations.length > MAX_OBJECTS) {
        Fail("The installer repository object count is invalid.");
    }
    const Entries = [];
    for (let Index = 0; Index < Declarations.length; Index++) {
        process.stdout.write(
            `installer repository step=compress item=${Index + 1}/${Declarations.length}\n`,
        );
        Entries.push(Readˉdeclaredˉfile(
            Declarations[Index].declaration,
            Declarations[Index].target,
        ));
    }
    Entries.sort((Left, Right) =>
        Compareˉtext(Left.component, Right.component) ||
        Compareˉtext(Left.target, Right.target) ||
        Compareˉtext(Left.path, Right.path));
    const LogicalKeys = new Set();
    for (const Entry of Entries) {
        const Key = `${Entry.component}|${Entry.target}|${Entry.path}`;
        if (LogicalKeys.has(Key)) Fail(`Duplicate repository object: ${Key}`);
        LogicalKeys.add(Key);
    }
    const Components = [...new Set(Entries.map(Entry => Entry.component))]
        .sort(Compareˉtext);
    if (Components.length > MAX_COMPONENTS) Fail("Too many installer repository components.");
    const Reachable = new Set(Profiles.flatMap(Profile => Profile.components));
    if (Components.some(Component => !Reachable.has(Component)) ||
        [...Reachable].some(Component => !Components.includes(Component))) {
        Fail("Repository profiles and object components differ.");
    }
    for (const Target of ["linux-x64", "windows-x64"]) {
        for (const Profile of Profiles) Selectionˉentries(Entries, Profile, Target);
    }
    const Blobs = new Map();
    for (const Entry of Entries) {
        const Existing = Blobs.get(Entry.blobSha256);
        if (Existing && !Existing.equals(Entry.packed)) {
            Fail("A repository blob digest collision was observed.");
        }
        Blobs.set(Entry.blobSha256, Entry.packed);
    }
    const Lines = [
        "windvale-installer-repository 1",
        `version ${Input.version}`,
        `channel ${Input.channel}`,
        `expanded-limit ${MAX_EXPANDED_SELECTION_BYTES}`,
        "target-count 2",
        "target linux-x64",
        "target windows-x64",
        `component-count ${Components.length}`,
        ...Components.map(Component => `component ${Component}`),
        `profile-count ${Profiles.length}`,
        ...Profiles.map(Profile =>
            `profile ${Profile.id} ${Profile.components.length} ${Profile.components.join(" ")}`),
        `object-count ${Entries.length}`,
        `blob-count ${Blobs.size}`,
        ...Entries.map(Entry =>
            `object ${Entry.component} ${Entry.target} ${Entry.path} ${Entry.mode} ` +
            `${Entry.sha256} ${Entry.bytes} ${Entry.encoding} ` +
            `${Entry.blobSha256} ${Entry.blobBytes}`),
    ];
    const IndexBytes = Buffer.from(`${Lines.join("\n")}\n`, "utf8");
    if (IndexBytes.length > MAX_INDEX_BYTES) Fail("The installer repository index is oversized.");
    const IndexSha256 = Sha256(IndexBytes);
    const BlobBytes = [...Blobs.values()].reduce((Total, Value) => Total + Value.length, 0);
    const Expected = Input.repository;
    if (Expected.expectedIndexSha256 !== undefined &&
        Expected.expectedIndexSha256 !== "pending" &&
        (Expected.expectedIndexSha256 !== IndexSha256 ||
         Expected.expectedIndexBytes !== IndexBytes.length ||
         Expected.expectedBlobCount !== Blobs.size ||
         Expected.expectedBlobBytes !== BlobBytes)) {
        Fail("The pinned installer repository identity changed.");
    }
    process.stdout.write("installer repository step=publish item=3/4\n");
    const ObjectRoot = path.join(Output, "Objects", "sha256");
    fs.mkdirSync(ObjectRoot, { recursive: true });
    for (const [Digest, Bytes] of [...Blobs.entries()].sort((Left, Right) =>
        Compareˉtext(Left[0], Right[0]))) {
        fs.writeFileSync(path.join(ObjectRoot, Digest), Bytes, { flag: "wx", mode: 0o644 });
    }
    // The index is the publication marker; readers cannot select a partial object set.
    fs.writeFileSync(path.join(Output, "Repository-Index.txt"), IndexBytes, {
        flag: "wx",
        mode: 0o644,
    });
    process.stdout.write("installer repository step=report item=4/4\n");
    process.stdout.write(
        `installer repository build status=Complete version=${Input.version} ` +
        `targets=2 profiles=${Profiles.length} objects=${Entries.length} ` +
        `blobs=${Blobs.size} blob-bytes=${BlobBytes} ` +
        `index-bytes=${IndexBytes.length} index-sha256=${IndexSha256}\n`,
    );
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "build" && (Arguments.length === 1 || Arguments.length === 2)) {
        Buildˉrepository(Arguments[0], Arguments[1]);
    } else {
        process.stderr.write(
            "Usage: node Build-Installer-Repository.mjs build output-directory [input]\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
