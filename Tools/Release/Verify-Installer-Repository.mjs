import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { gunzipSync } from "node:zlib";

const IDENTIFIER = /^[a-z0-9](?:[a-z0-9.-]*[a-z0-9])?$/;
const SAFE_PATH = /^[A-Za-z0-9._/-]+$/;
const SHA256 = /^[0-9a-f]{64}$/;
const VERSION = /^0\.[0-9]+\.[0-9]+(?:-dev\.[0-9]+)?$/;
const MAX_INDEX_BYTES = 1024 * 1024;
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

function Parseˉdecimal(Value, Description, Maximum = Number.MAX_SAFE_INTEGER) {
    if (!/^(?:0|[1-9][0-9]*)$/.test(Value)) Fail(`Invalid ${Description}.`);
    const Parsed = Number(Value);
    if (!Number.isSafeInteger(Parsed) || Parsed > Maximum) Fail(`Oversized ${Description}.`);
    return Parsed;
}

function Assertˉidentifier(Value, Description) {
    if (Value.length > 64 || !IDENTIFIER.test(Value)) Fail(`Invalid ${Description}.`);
    return Value;
}

function Assertˉpath(Value) {
    if (Value.length > 1024 || !SAFE_PATH.test(Value) || Value.startsWith("/") ||
        Value.endsWith("/") || Value.includes("//") ||
        Value.split("/").some(Part => Part === "." || Part === "..")) {
        Fail("Invalid installer repository object path.");
    }
    return Value;
}

function Assertˉorderedˉunique(Values, Description) {
    for (let Index = 1; Index < Values.length; Index++) {
        if (Compareˉtext(Values[Index - 1], Values[Index]) >= 0) {
            Fail(`${Description} are not canonical.`);
        }
    }
}

function Readˉindex(RepositoryPath) {
    const Stat = fs.lstatSync(RepositoryPath);
    if (!Stat.isDirectory() || Stat.isSymbolicLink()) {
        Fail("The installer repository root is not an ordinary directory.");
    }
    const Root = fs.realpathSync(RepositoryPath);
    const IndexPath = path.join(Root, "Repository-Index.txt");
    const IndexStat = fs.lstatSync(IndexPath);
    if (!IndexStat.isFile() || IndexStat.isSymbolicLink() ||
        IndexStat.size < 1 || IndexStat.size > MAX_INDEX_BYTES) {
        Fail("The installer repository index is missing or oversized.");
    }
    const Bytes = fs.readFileSync(IndexPath);
    const Text = Bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Bytes) || Text.includes("\0") ||
        Text.includes("\r") || !Text.endsWith("\n")) {
        Fail("The installer repository index is not canonical UTF-8 text.");
    }
    const Lines = Text.slice(0, -1).split("\n");
    if (Lines.some(Line => Line.length === 0 || Line.trim() !== Line || Line.includes("  "))) {
        Fail("The installer repository index contains a noncanonical line.");
    }
    let Cursor = 0;
    function Take(Description) {
        if (Cursor >= Lines.length) Fail(`Missing ${Description}.`);
        return Lines[Cursor++];
    }
    if (Take("repository header") !== "windvale-installer-repository 1") {
        Fail("Invalid installer repository header.");
    }
    const VersionMatch = /^version (.+)$/.exec(Take("version"));
    const ChannelMatch = /^channel (development|stable)$/.exec(Take("channel"));
    const LimitMatch = /^expanded-limit ([0-9]+)$/.exec(Take("expanded limit"));
    if (!VersionMatch || !VERSION.test(VersionMatch[1]) || !ChannelMatch || !LimitMatch) {
        Fail("Invalid installer repository identity records.");
    }
    const ExpandedLimit = Parseˉdecimal(
        LimitMatch[1],
        "expanded limit",
        MAX_OBJECT_BYTES,
    );
    if (ExpandedLimit < 1) Fail("The installer repository expanded limit is zero.");
    const TargetCountMatch = /^target-count ([0-9]+)$/.exec(Take("target count"));
    if (!TargetCountMatch || Parseˉdecimal(TargetCountMatch[1], "target count", 16) !== 2 ||
        Take("Linux target") !== "target linux-x64" ||
        Take("Windows target") !== "target windows-x64") {
        Fail("The installer repository target inventory differs.");
    }
    const ComponentCountMatch = /^component-count ([0-9]+)$/.exec(Take("component count"));
    if (!ComponentCountMatch) Fail("Invalid installer repository component count.");
    const ComponentCount = Parseˉdecimal(
        ComponentCountMatch[1],
        "component count",
        MAX_COMPONENTS,
    );
    if (ComponentCount < 1) Fail("The installer repository has no components.");
    const Components = [];
    for (let Index = 0; Index < ComponentCount; Index++) {
        const Match = /^component ([^ ]+)$/.exec(Take("component"));
        if (!Match) Fail("Invalid installer repository component record.");
        Components.push(Assertˉidentifier(Match[1], "component identifier"));
    }
    Assertˉorderedˉunique(Components, "Installer repository components");
    const ProfileCountMatch = /^profile-count ([0-9]+)$/.exec(Take("profile count"));
    if (!ProfileCountMatch) Fail("Invalid installer repository profile count.");
    const ProfileCount = Parseˉdecimal(ProfileCountMatch[1], "profile count", MAX_PROFILES);
    if (ProfileCount < 1) Fail("The installer repository has no profiles.");
    const Profiles = [];
    for (let Index = 0; Index < ProfileCount; Index++) {
        const Fields = Take("profile").split(" ");
        if (Fields.length < 4 || Fields[0] !== "profile") {
            Fail("Invalid installer repository profile record.");
        }
        const Id = Assertˉidentifier(Fields[1], "profile identifier");
        const Count = Parseˉdecimal(Fields[2], "profile component count", MAX_COMPONENTS);
        const ProfileComponents = Fields.slice(3).map(Value =>
            Assertˉidentifier(Value, "profile component"));
        if (Count !== ProfileComponents.length || Count < 1) {
            Fail("The installer repository profile width differs.");
        }
        Assertˉorderedˉunique(ProfileComponents, `Profile ${Id} components`);
        Profiles.push({ id: Id, components: ProfileComponents });
    }
    Assertˉorderedˉunique(Profiles.map(Profile => Profile.id), "Repository profiles");
    const ObjectCountMatch = /^object-count ([0-9]+)$/.exec(Take("object count"));
    const BlobCountMatch = /^blob-count ([0-9]+)$/.exec(Take("blob count"));
    if (!ObjectCountMatch || !BlobCountMatch) Fail("Invalid repository object inventory.");
    const ObjectCount = Parseˉdecimal(ObjectCountMatch[1], "object count", MAX_OBJECTS);
    const DeclaredBlobCount = Parseˉdecimal(BlobCountMatch[1], "blob count", MAX_OBJECTS);
    if (ObjectCount < 1 || DeclaredBlobCount < 1 || DeclaredBlobCount > ObjectCount) {
        Fail("The repository object or blob count is empty or inconsistent.");
    }
    const Entries = [];
    const Keys = [];
    const Blobs = new Map();
    for (let Index = 0; Index < ObjectCount; Index++) {
        const Fields = Take("object").split(" ");
        if (Fields.length !== 10 || Fields[0] !== "object") {
            Fail("Invalid installer repository object record width.");
        }
        const Component = Assertˉidentifier(Fields[1], "object component");
        const Target = Fields[2];
        if (!["all", "linux-x64", "windows-x64"].includes(Target)) {
            Fail("Invalid installer repository object target.");
        }
        const ObjectPath = Assertˉpath(Fields[3]);
        if (!/^0[0-7]{3}$/.test(Fields[4]) || !SHA256.test(Fields[5]) ||
            Fields[7] !== "gzip-1" || !SHA256.test(Fields[8])) {
            Fail("Invalid installer repository object identity.");
        }
        const RawBytes = Parseˉdecimal(Fields[6], "expanded object bytes", MAX_OBJECT_BYTES);
        const BlobBytes = Parseˉdecimal(Fields[9], "compressed object bytes", MAX_OBJECT_BYTES);
        if (BlobBytes < 1) Fail("An installer repository blob is empty.");
        const Entry = {
            component: Component,
            target: Target,
            path: ObjectPath,
            mode: Fields[4],
            sha256: Fields[5],
            bytes: RawBytes,
            encoding: Fields[7],
            blobSha256: Fields[8],
            blobBytes: BlobBytes,
        };
        const Key = `${Component}|${Target}|${ObjectPath}`;
        Keys.push(Key);
        const Existing = Blobs.get(Entry.blobSha256);
        if (Existing && (Existing.blobBytes !== Entry.blobBytes ||
            Existing.sha256 !== Entry.sha256 || Existing.bytes !== Entry.bytes)) {
            Fail("One repository blob has inconsistent expanded identity.");
        }
        Blobs.set(Entry.blobSha256, Entry);
        Entries.push(Entry);
    }
    if (Cursor !== Lines.length) Fail("The installer repository index has trailing records.");
    Assertˉorderedˉunique(Keys, "Installer repository objects");
    if (Blobs.size !== DeclaredBlobCount) Fail("The installer repository blob count differs.");
    const ObservedComponents = [...new Set(Entries.map(Entry => Entry.component))]
        .sort(Compareˉtext);
    const ReachableComponents = [...new Set(Profiles.flatMap(Profile => Profile.components))]
        .sort(Compareˉtext);
    if (ObservedComponents.join("|") !== Components.join("|") ||
        ReachableComponents.join("|") !== Components.join("|")) {
        Fail("Repository components, profiles, and objects differ.");
    }
    for (const Target of ["linux-x64", "windows-x64"]) {
        for (const Profile of Profiles) Selectˉentries(
            { entries: Entries, expandedLimit: ExpandedLimit },
            Target,
            Profile,
        );
    }
    return {
        root: Root,
        bytes: Bytes,
        sha256: Sha256(Bytes),
        version: VersionMatch[1],
        channel: ChannelMatch[1],
        expandedLimit: ExpandedLimit,
        components: Components,
        profiles: Profiles,
        entries: Entries,
        blobs: Blobs,
    };
}

function Selectˉentries(Index, Target, Profile) {
    const ComponentSet = new Set(Profile.components);
    const Selected = Index.entries.filter(Entry =>
        ComponentSet.has(Entry.component) &&
        (Entry.target === "all" || Entry.target === Target));
    for (const Component of Profile.components) {
        if (!Selected.some(Entry => Entry.component === Component)) {
            Fail(`Profile ${Profile.id} has no ${Target} object for ${Component}.`);
        }
    }
    const Paths = new Set();
    let ExpandedBytes = 0;
    for (const Entry of Selected) {
        if (Paths.has(Entry.path)) Fail(`Selected installation path repeats: ${Entry.path}`);
        Paths.add(Entry.path);
        ExpandedBytes += Entry.bytes;
    }
    if (ExpandedBytes > Index.expandedLimit) Fail("Selected profile exceeds its expanded limit.");
    return { entries: Selected, expandedBytes: ExpandedBytes };
}

function Inventoryˉrepository(Index) {
    const Expected = new Set([
        "Repository-Index.txt",
        "Objects/",
        "Objects/sha256/",
        ...[...Index.blobs.keys()].map(Digest => `Objects/sha256/${Digest}`),
    ]);
    const Observed = new Set();
    function Visit(Current, Prefix) {
        for (const Entry of fs.readdirSync(Current, { withFileTypes: true })) {
            const Relative = Prefix ? `${Prefix}/${Entry.name}` : Entry.name;
            const FullPath = path.join(Current, Entry.name);
            const Stat = fs.lstatSync(FullPath);
            if (Stat.isSymbolicLink()) Fail(`Repository inventory contains a link: ${Relative}`);
            if (Stat.isDirectory()) {
                Observed.add(`${Relative}/`);
                Visit(FullPath, Relative);
            } else if (Stat.isFile()) {
                Observed.add(Relative);
            } else {
                Fail(`Repository inventory contains a special entry: ${Relative}`);
            }
        }
    }
    Visit(Index.root, "");
    if (Expected.size !== Observed.size ||
        [...Expected].some(Value => !Observed.has(Value))) {
        Fail("The installer repository complete inventory differs.");
    }
}

function Verifyˉrepository(RepositoryPath) {
    const Index = Readˉindex(RepositoryPath);
    Inventoryˉrepository(Index);
    let BlobBytes = 0;
    for (const [Digest, Entry] of Index.blobs) {
        const BlobPath = path.join(Index.root, "Objects", "sha256", Digest);
        const Stat = fs.lstatSync(BlobPath);
        if (!Stat.isFile() || Stat.isSymbolicLink() || Stat.size !== Entry.blobBytes) {
            Fail(`Repository blob length differs: ${Digest}`);
        }
        const Packed = fs.readFileSync(BlobPath);
        if (Sha256(Packed) !== Digest) Fail(`Repository blob digest differs: ${Digest}`);
        let Expanded;
        try {
            Expanded = gunzipSync(Packed, { maxOutputLength: Entry.bytes });
        } catch {
            Fail(`Repository blob decompression failed: ${Digest}`);
        }
        if (Expanded.length !== Entry.bytes || Sha256(Expanded) !== Entry.sha256) {
            Fail(`Repository expanded object identity differs: ${Digest}`);
        }
        BlobBytes += Packed.length;
    }
    process.stdout.write(
        `installer repository verify status=Valid version=${Index.version} ` +
        `targets=2 profiles=${Index.profiles.length} objects=${Index.entries.length} ` +
        `blobs=${Index.blobs.size} blob-bytes=${BlobBytes} ` +
        `index-sha256=${Index.sha256}\n`,
    );
}

function Selectˉprofile(RepositoryPath, Target, ProfileId) {
    if (!["linux-x64", "windows-x64"].includes(Target)) {
        Fail(`Unsupported installer repository target: ${Target}`);
    }
    const Index = Readˉindex(RepositoryPath);
    const Profile = Index.profiles.find(Current => Current.id === ProfileId);
    if (!Profile) Fail(`Unknown installer repository profile: ${ProfileId}`);
    const Selection = Selectˉentries(Index, Target, Profile);
    const BlobIds = new Set();
    let DownloadBytes = 0;
    for (const Entry of Selection.entries) {
        if (!BlobIds.has(Entry.blobSha256)) {
            BlobIds.add(Entry.blobSha256);
            DownloadBytes += Entry.blobBytes;
        }
    }
    const Lines = [
        "windvale-installer-selection 1",
        `version ${Index.version}`,
        `target ${Target}`,
        `profile ${Profile.id}`,
        `object-count ${Selection.entries.length}`,
        `blob-count ${BlobIds.size}`,
        `download-bytes ${DownloadBytes}`,
        `expanded-bytes ${Selection.expandedBytes}`,
        ...Selection.entries.map(Entry =>
            `object ${Entry.component} ${Entry.path} ${Entry.mode} ` +
            `${Entry.sha256} ${Entry.bytes} ${Entry.encoding} ` +
            `${Entry.blobSha256} ${Entry.blobBytes}`),
    ];
    process.stdout.write(`${Lines.join("\n")}\n`);
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "verify" && Arguments.length === 1) {
        Verifyˉrepository(Arguments[0]);
    } else if (Command === "select" && Arguments.length === 3) {
        Selectˉprofile(Arguments[0], Arguments[1], Arguments[2]);
    } else {
        process.stderr.write(
            "Usage: node Verify-Installer-Repository.mjs " +
            "<verify repository|select repository target profile>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
