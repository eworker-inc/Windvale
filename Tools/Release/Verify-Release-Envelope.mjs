import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

const SHA256 = /^[0-9a-f]{64}$/;
const GIT_ID = /^[0-9a-f]{40}$/;
const VERSION = /^0\.[0-9]+\.[0-9]+$/;
const TOKEN = /^[a-z0-9][a-z0-9.-]*$/;
const SAFE_PATH = /^[A-Za-z0-9._/-]+$/;
const MAX_ARTIFACT_BYTES = 268_435_456;
const MAX_RELEASE_BYTES = 536_870_912;
const MAX_KEY_BYTES = 16_384;
const MAX_ROOT_POLICY_BYTES = 16_384;
const MAX_SIGNATURE_BYTES = 4_096;
const MAX_MANIFEST_BYTES = 8_388_608;
const MAX_PATH_BYTES = 1_024;
const MAX_PATH_PARTS = 32;
const MAX_TOKEN_BYTES = 64;
const MAX_INVENTORY_ENTRIES = 8_192;
const DOMAIN_PREFIX = Buffer.from("windvale-release-signature-v1\0", "ascii");
const REQUIRED_PROFILE = new Set([
    "approval|all",
    "installer|linux-x64",
    "installer|windows-x64",
    "license|all",
    "package|all",
    "provenance|all",
    "qualification|linux-x64",
    "qualification|windows-x64",
    "recovery|stage0",
    "source|all",
    "verifier|all",
]);

function Fail(Message) {
    throw new Error(Message);
}

function Sha256(Value) {
    return crypto.createHash("sha256").update(Value).digest("hex");
}

function Readˉordinaryˉfile(FilePath, Description, MaximumBytes) {
    let Stat;
    try {
        Stat = fs.lstatSync(FilePath);
    } catch {
        Fail(`${Description} is missing.`);
    }
    if (!Stat.isFile() || Stat.isSymbolicLink()) Fail(`${Description} is not an ordinary file.`);
    if (!Number.isSafeInteger(Stat.size) || Stat.size < 0 ||
        (MaximumBytes !== undefined && Stat.size > MaximumBytes)) {
        Fail(`${Description} exceeds its byte limit.`);
    }
    const Bytes = fs.readFileSync(FilePath);
    if (Bytes.length !== Stat.size) Fail(`${Description} changed while it was read.`);
    return Bytes;
}

function Strictˉlines(Bytes, Description) {
    const Text = Bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Bytes) || Text.includes("\r") ||
        Text.includes("\0") || !Text.endsWith("\n")) {
        Fail(`${Description} is not canonical LF UTF-8.`);
    }
    return Text.slice(0, -1).split("\n");
}

function Decimal(Text, Description, Maximum = Number.MAX_SAFE_INTEGER) {
    if (!/^[1-9][0-9]*$/.test(Text)) Fail(`Invalid ${Description}.`);
    const Value = Number(Text);
    if (!Number.isSafeInteger(Value) || Value < 1 || Value > Maximum) {
        Fail(`Invalid ${Description}.`);
    }
    return Value;
}

function Safeˉpath(Value) {
    return typeof Value === "string" && SAFE_PATH.test(Value) &&
        !Value.startsWith("/") && !Value.endsWith("/") && !Value.includes("//") &&
        Buffer.byteLength(Value, "utf8") <= MAX_PATH_BYTES &&
        Value.split("/").length <= MAX_PATH_PARTS &&
        !Value.split("/").some(Part => Part === "." || Part === "..");
}

function Publicˉkeyˉfromˉder(Encoded, ExpectedId, Description) {
    const Der = Buffer.from(Encoded, "base64");
    if (Der.toString("base64") !== Encoded || Sha256(Der) !== ExpectedId) {
        Fail(`${Description} identity differs.`);
    }
    const Key = crypto.createPublicKey({ key: Der, format: "der", type: "spki" });
    if (Key.type !== "public" || Key.asymmetricKeyType !== "ed25519") {
        Fail(`${Description} is not Ed25519.`);
    }
    const CanonicalDer = Key.export({ type: "spki", format: "der" });
    if (!CanonicalDer.equals(Der)) Fail(`${Description} encoding is not canonical.`);
    return Key;
}

function Publicˉkeyˉidentity(Key) {
    if (Key.type !== "public" || Key.asymmetricKeyType !== "ed25519") {
        Fail("The trusted root is not an Ed25519 public key.");
    }
    return Sha256(Key.export({ type: "spki", format: "der" }));
}

function Signingˉmessage(Kind, Message) {
    return Buffer.concat([DOMAIN_PREFIX, Buffer.from(`${Kind}\0`, "ascii"), Message]);
}

function Verifyˉsignature(Bytes, Kind, KeyId, Message, Key) {
    const Lines = Strictˉlines(Bytes, `${Kind} signature`);
    if (Lines.length !== 6 || Lines[0] !== "windvale-signature 1" ||
        Lines[1] !== `kind ${Kind}` || Lines[2] !== "algorithm ed25519" ||
        Lines[3] !== `key ${KeyId}` || Lines[4] !== `message-sha256 ${Sha256(Message)}`) {
        Fail(`${Kind} signature metadata differs.`);
    }
    const Match = /^signature ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[5]);
    if (!Match) Fail(`${Kind} signature encoding differs.`);
    const Signature = Buffer.from(Match[1], "base64");
    if (Signature.length !== 64 || Signature.toString("base64") !== Match[1] ||
        !crypto.verify(null, Signingˉmessage(Kind, Message), Key, Signature)) {
        Fail(`${Kind} signature is invalid.`);
    }
}

function Readˉroot(ReleaseRoot, TrustedRootPath) {
    const PolicyBytes = Readˉordinaryˉfile(
        path.join(ReleaseRoot, "Root-Policy.txt"),
        "Root policy",
        MAX_ROOT_POLICY_BYTES,
    );
    const SignatureBytes = Readˉordinaryˉfile(
        path.join(ReleaseRoot, "Root-Policy.sig"),
        "Root signature",
        MAX_SIGNATURE_BYTES,
    );
    const Lines = Strictˉlines(PolicyBytes, "Root policy");
    if (Lines.length !== 8 || Lines[0] !== "windvale-release-root 1" ||
        Lines[2] !== "algorithm ed25519") {
        Fail("Root policy structure differs.");
    }
    const Generation = /^policy-generation ([1-9][0-9]*)$/.exec(Lines[1]);
    const RootRecord = /^root-key ([0-9a-f]{64}) ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[3]);
    const ReleaseRecord = /^release-key ([0-9a-f]{64}) ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[4]);
    const VersionPrefix = /^release-version-prefix (0\.[0-9]+\.)$/.exec(Lines[5]);
    const Minimum = /^release-sequence-min ([1-9][0-9]*)$/.exec(Lines[6]);
    const Maximum = /^release-sequence-max ([1-9][0-9]*)$/.exec(Lines[7]);
    if (!Generation || !RootRecord || !ReleaseRecord || !VersionPrefix ||
        !Minimum || !Maximum) {
        Fail("Root policy record differs.");
    }
    const TrustedRootBytes = Readˉordinaryˉfile(
        TrustedRootPath,
        "Trusted root public key",
        MAX_KEY_BYTES,
    );
    const TrustedRoot = crypto.createPublicKey(TrustedRootBytes);
    const TrustedId = Publicˉkeyˉidentity(TrustedRoot);
    const EmbeddedRoot = Publicˉkeyˉfromˉder(RootRecord[2], RootRecord[1], "Embedded root key");
    if (TrustedId !== RootRecord[1] ||
        !TrustedRoot.export({ type: "spki", format: "der" }).equals(
            EmbeddedRoot.export({ type: "spki", format: "der" }),
        )) {
        Fail("The root policy does not descend from the trusted root.");
    }
    Verifyˉsignature(SignatureBytes, "root-policy", TrustedId, PolicyBytes, TrustedRoot);
    const ReleaseKey = Publicˉkeyˉfromˉder(
        ReleaseRecord[2],
        ReleaseRecord[1],
        "Delegated release key",
    );
    const MinimumValue = Decimal(Minimum[1], "minimum release sequence");
    const MaximumValue = Decimal(Maximum[1], "maximum release sequence");
    if (MinimumValue > MaximumValue) Fail("Root policy sequence range is reversed.");
    return {
        policyBytes: PolicyBytes,
        generation: Decimal(Generation[1], "policy generation"),
        versionPrefix: VersionPrefix[1],
        minimum: MinimumValue,
        maximum: MaximumValue,
        releaseKeyId: ReleaseRecord[1],
        releaseKey: ReleaseKey,
    };
}

function Readˉmanifest(ReleaseRoot, Policy, MinimumSequence) {
    const ManifestBytes = Readˉordinaryˉfile(
        path.join(ReleaseRoot, "Release-Manifest.txt"),
        "Release manifest",
        MAX_MANIFEST_BYTES,
    );
    const SignatureBytes = Readˉordinaryˉfile(
        path.join(ReleaseRoot, "Release-Manifest.sig"),
        "Release manifest signature",
        MAX_SIGNATURE_BYTES,
    );
    Verifyˉsignature(
        SignatureBytes,
        "release-manifest",
        Policy.releaseKeyId,
        ManifestBytes,
        Policy.releaseKey,
    );
    const Lines = Strictˉlines(ManifestBytes, "Release manifest");
    if (Lines.length < 10 || Lines[0] !== "windvale-release-manifest 1") {
        Fail("Release manifest structure differs.");
    }
    const Version = /^version (0\.[0-9]+\.[0-9]+)$/.exec(Lines[1]);
    const Channel = /^channel ([a-z0-9][a-z0-9.-]*)$/.exec(Lines[2]);
    const Sequence = /^sequence ([1-9][0-9]*)$/.exec(Lines[3]);
    const Revision = /^revision ([0-9a-f]{40})$/.exec(Lines[4]);
    const Tree = /^tree ([0-9a-f]{40})$/.exec(Lines[5]);
    const PolicyHash = /^root-policy-sha256 ([0-9a-f]{64})$/.exec(Lines[6]);
    const ReleaseKey = /^release-key ([0-9a-f]{64})$/.exec(Lines[7]);
    const ArtifactCount = /^artifact-count ([1-9][0-9]*)$/.exec(Lines[8]);
    if (!Version || !Channel || !Sequence || !Revision || !Tree || !PolicyHash ||
        !ReleaseKey || !ArtifactCount || !VERSION.test(Version[1]) ||
        Channel[1] !== "preview" || !GIT_ID.test(Revision[1]) || !GIT_ID.test(Tree[1]) ||
        PolicyHash[1] !== Sha256(Policy.policyBytes) || ReleaseKey[1] !== Policy.releaseKeyId) {
        Fail("Release manifest header differs.");
    }
    const SequenceValue = Decimal(Sequence[1], "release sequence");
    if (!Version[1].startsWith(Policy.versionPrefix) || SequenceValue < Policy.minimum ||
        SequenceValue > Policy.maximum || SequenceValue < MinimumSequence) {
        Fail("Release manifest is outside the accepted sequence or version policy.");
    }
    const Count = Decimal(ArtifactCount[1], "artifact count", 4096);
    if (Lines.length !== 9 + Count || Count < REQUIRED_PROFILE.size) {
        Fail("Release artifact count differs.");
    }
    const Artifacts = [];
    let TotalBytes = 0;
    let PreviousOrder = null;
    const Paths = new Set();
    const CaseFoldedPaths = new Set();
    const RoleTargets = new Set();
    for (const Line of Lines.slice(9)) {
        const Match = /^artifact ([a-z0-9][a-z0-9.-]*) ([a-z0-9][a-z0-9.-]*) ([0-9a-f]{64}) ([1-9][0-9]*) ([A-Za-z0-9._/-]+)$/.exec(Line);
        if (!Match || !TOKEN.test(Match[1]) || !TOKEN.test(Match[2]) ||
            Buffer.byteLength(Match[1], "utf8") > MAX_TOKEN_BYTES ||
            Buffer.byteLength(Match[2], "utf8") > MAX_TOKEN_BYTES ||
            !SHA256.test(Match[3]) || !Safeˉpath(Match[5]) ||
            !Match[5].startsWith("Artifacts/")) {
            Fail("Release artifact record differs.");
        }
        const Bytes = Decimal(Match[4], "artifact byte length", MAX_ARTIFACT_BYTES);
        TotalBytes += Bytes;
        if (!Number.isSafeInteger(TotalBytes) || TotalBytes > MAX_RELEASE_BYTES) {
            Fail("Release artifact bytes exceed the bounded profile.");
        }
        const Order = Buffer.from(`${Match[1]}\0${Match[2]}\0${Match[5]}`);
        if (PreviousOrder && PreviousOrder.compare(Order) >= 0) {
            Fail("Release artifacts are not canonically ordered.");
        }
        PreviousOrder = Order;
        const RoleTarget = `${Match[1]}|${Match[2]}`;
        const FoldedPath = Match[5].toLowerCase();
        if (Paths.has(Match[5]) || CaseFoldedPaths.has(FoldedPath) ||
            RoleTargets.has(RoleTarget)) {
            Fail("Release artifacts contain a duplicate path or role/target pair.");
        }
        Paths.add(Match[5]);
        CaseFoldedPaths.add(FoldedPath);
        RoleTargets.add(RoleTarget);
        Artifacts.push({
            role: Match[1],
            target: Match[2],
            sha256: Match[3],
            bytes: Bytes,
            path: Match[5],
        });
    }
    for (const Required of REQUIRED_PROFILE) {
        if (!RoleTargets.has(Required)) Fail(`Release profile is missing ${Required}.`);
    }
    return {
        bytes: ManifestBytes,
        version: Version[1],
        sequence: SequenceValue,
        revision: Revision[1],
        tree: Tree[1],
        artifacts: Artifacts,
        totalBytes: TotalBytes,
    };
}

function Inventoryˉentries(Root) {
    const Files = [];
    const Directories = [];
    let Count = 0;
    function Visit(Directory, Prefix, Depth) {
        if (Depth > MAX_PATH_PARTS) Fail("Release directory nesting exceeds its limit.");
        const Handle = fs.opendirSync(Directory);
        try {
            for (let Entry = Handle.readSync(); Entry !== null; Entry = Handle.readSync()) {
                const Relative = Prefix ? `${Prefix}/${Entry.name}` : Entry.name;
                Count++;
                if (Count > MAX_INVENTORY_ENTRIES || !Safeˉpath(Relative)) {
                    Fail("Release directory inventory exceeds its bounded path or entry profile.");
                }
                const Candidate = path.join(Directory, Entry.name);
                const Stat = fs.lstatSync(Candidate);
                if (Stat.isSymbolicLink()) {
                    Fail(`Release directory contains a link: ${Relative}`);
                }
                if (Stat.isDirectory()) {
                    Directories.push(Relative);
                    Visit(Candidate, Relative, Depth + 1);
                } else if (Stat.isFile()) {
                    Files.push(Relative);
                } else {
                    Fail(`Release directory contains a special entry: ${Relative}`);
                }
            }
        } finally {
            Handle.closeSync();
        }
    }
    Visit(Root, "", 0);
    const Compare = (Left, Right) => Buffer.from(Left).compare(Buffer.from(Right));
    return { files: Files.sort(Compare), directories: Directories.sort(Compare) };
}

function Verifyˉrelease(TrustedRootPath, ReleasePath, MinimumSequenceText) {
    const ReleaseStat = fs.lstatSync(ReleasePath);
    if (!ReleaseStat.isDirectory() || ReleaseStat.isSymbolicLink()) {
        Fail("Release root must be an ordinary directory.");
    }
    const ReleaseRoot = fs.realpathSync(ReleasePath);
    const MinimumSequence = MinimumSequenceText === undefined ? 1 :
        Decimal(MinimumSequenceText, "required minimum sequence");
    process.stdout.write("release verify step=verify-root item=1/4\n");
    const Policy = Readˉroot(ReleaseRoot, TrustedRootPath);
    process.stdout.write("release verify step=verify-manifest item=2/4\n");
    const Manifest = Readˉmanifest(ReleaseRoot, Policy, MinimumSequence);
    process.stdout.write(
        `release verify step=verify-artifacts item=3/4 count=${Manifest.artifacts.length}\n`,
    );
    for (const Artifact of Manifest.artifacts) {
        const Candidate = path.resolve(ReleaseRoot, ...Artifact.path.split("/"));
        if (!Candidate.startsWith(`${ReleaseRoot}${path.sep}`)) {
            Fail("A release artifact escapes the release root.");
        }
        const Bytes = Readˉordinaryˉfile(
            Candidate,
            `Artifact ${Artifact.path}`,
            MAX_ARTIFACT_BYTES,
        );
        if (Bytes.length !== Artifact.bytes || Sha256(Bytes) !== Artifact.sha256) {
            Fail(`Release artifact identity differs: ${Artifact.path}`);
        }
    }
    const ExpectedFiles = [
        "Release-Manifest.sig",
        "Release-Manifest.txt",
        "Root-Policy.sig",
        "Root-Policy.txt",
        ...Manifest.artifacts.map(Artifact => Artifact.path),
    ].sort((Left, Right) => Buffer.from(Left).compare(Buffer.from(Right)));
    const ExpectedDirectories = new Set();
    for (const Artifact of Manifest.artifacts) {
        const Parts = Artifact.path.split("/");
        for (let Length = 1; Length < Parts.length; Length++) {
            ExpectedDirectories.add(Parts.slice(0, Length).join("/"));
        }
    }
    const Compare = (Left, Right) => Buffer.from(Left).compare(Buffer.from(Right));
    const ExpectedDirectoryList = [...ExpectedDirectories].sort(Compare);
    const Observed = Inventoryˉentries(ReleaseRoot);
    if (ExpectedFiles.join("\0") !== Observed.files.join("\0") ||
        ExpectedDirectoryList.join("\0") !== Observed.directories.join("\0")) {
        Fail("The release directory contains an undeclared or missing file.");
    }
    process.stdout.write("release verify step=report item=4/4\n");
    process.stdout.write(
        `release verify status=Valid version=${Manifest.version} sequence=${Manifest.sequence} ` +
        `revision=${Manifest.revision} tree=${Manifest.tree} artifacts=${Manifest.artifacts.length} ` +
        `bytes=${Manifest.totalBytes} manifest=${Sha256(Manifest.bytes)} ` +
        `policy-generation=${Policy.generation}\n`,
    );
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "verify" && (Arguments.length === 2 || Arguments.length === 3)) {
        Verifyˉrelease(Arguments[0], Arguments[1], Arguments[2]);
    } else {
        process.stderr.write(
            "Usage: node Verify-Release-Envelope.mjs verify " +
            "<trusted-root-public-key> <release-directory> [minimum-sequence]\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
