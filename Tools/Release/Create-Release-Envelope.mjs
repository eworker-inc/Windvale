import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

const SHA256 = /^[0-9a-f]{64}$/;
const GIT_ID = /^[0-9a-f]{40}$/;
const VERSION = /^0\.[0-9]+\.[0-9]+$/;
const KEY_NAME = /^(?:root|release)$/;
const SAFE_PATH = /^[A-Za-z0-9._/-]+$/;
const TOKEN = /^[a-z0-9][a-z0-9.-]*$/;
const MAX_ARTIFACT_BYTES = 268_435_456;
const MAX_RELEASE_BYTES = 536_870_912;
const MAX_KEY_BYTES = 16_384;
const MAX_ROOT_INPUT_BYTES = 16_384;
const MAX_ROOT_POLICY_BYTES = 16_384;
const MAX_SIGNATURE_BYTES = 4_096;
const MAX_RELEASE_INPUT_BYTES = 8_388_608;
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
    const Stat = fs.lstatSync(FilePath);
    if (!Stat.isFile() || Stat.isSymbolicLink()) {
        Fail(`${Description} must be an ordinary file.`);
    }
    if (!Number.isSafeInteger(Stat.size) || Stat.size < 0 ||
        (MaximumBytes !== undefined && Stat.size > MaximumBytes)) {
        Fail(`${Description} exceeds its byte limit.`);
    }
    const Bytes = fs.readFileSync(FilePath);
    if (Bytes.length !== Stat.size) Fail(`${Description} changed while it was read.`);
    return Bytes;
}

function Readˉcanonicalˉtext(FilePath, Description, MaximumBytes) {
    const Bytes = Readˉordinaryˉfile(FilePath, Description, MaximumBytes);
    const Text = Bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Bytes) || Text.includes("\r") ||
        Text.includes("\0") || !Text.endsWith("\n")) {
        Fail(`${Description} must be canonical LF UTF-8 text.`);
    }
    return Text;
}

function Assertˉexactˉkeys(Value, Expected, Description) {
    if (Value === null || typeof Value !== "object" || Array.isArray(Value) ||
        Object.keys(Value).sort().join("|") !== [...Expected].sort().join("|")) {
        Fail(`${Description} fields differ.`);
    }
}

function Parseˉpositiveˉinteger(Value, Description, Maximum = Number.MAX_SAFE_INTEGER) {
    if (!Number.isSafeInteger(Value) || Value < 1 || Value > Maximum) {
        Fail(`Invalid ${Description}: ${Value}`);
    }
    return Value;
}

function Parseˉdecimal(Text, Description, Maximum = Number.MAX_SAFE_INTEGER) {
    if (!/^[1-9][0-9]*$/.test(Text)) Fail(`Invalid ${Description}.`);
    return Parseˉpositiveˉinteger(Number(Text), Description, Maximum);
}

function Assertˉordinaryˉrelativeˉpath(Value, Description) {
    if (typeof Value !== "string" || !SAFE_PATH.test(Value) ||
        Value.startsWith("/") || Value.endsWith("/") || Value.includes("//") ||
        Buffer.byteLength(Value, "utf8") > MAX_PATH_BYTES ||
        Value.split("/").length > MAX_PATH_PARTS ||
        Value.split("/").some(Part => Part === "." || Part === "..")) {
        Fail(`Invalid ${Description}: ${Value}`);
    }
    return Value;
}

function Assertˉemptyˉdirectory(DirectoryPath, Description) {
    const Stat = fs.lstatSync(DirectoryPath);
    if (!Stat.isDirectory() || Stat.isSymbolicLink() ||
        fs.readdirSync(DirectoryPath).length !== 0) {
        Fail(`${Description} must be an existing empty ordinary directory.`);
    }
    return fs.realpathSync(DirectoryPath);
}

function Assertˉordinaryˉdirectory(DirectoryPath, Description) {
    const Stat = fs.lstatSync(DirectoryPath);
    if (!Stat.isDirectory() || Stat.isSymbolicLink()) {
        Fail(`${Description} must be an ordinary directory.`);
    }
    return fs.realpathSync(DirectoryPath);
}

function Publicˉinformation(Key) {
    const PublicKey = Key.type === "private" ? crypto.createPublicKey(Key) : Key;
    if (PublicKey.type !== "public" || PublicKey.asymmetricKeyType !== "ed25519") {
        Fail("A release key is not Ed25519.");
    }
    const Der = PublicKey.export({ type: "spki", format: "der" });
    return { key: PublicKey, der: Der, id: Sha256(Der) };
}

function Readˉprivateˉkey(FilePath) {
    const Key = crypto.createPrivateKey(
        Readˉordinaryˉfile(FilePath, "Private key", MAX_KEY_BYTES),
    );
    if (Key.type !== "private" || Key.asymmetricKeyType !== "ed25519") {
        Fail("The private key is not Ed25519.");
    }
    return Key;
}

function Readˉpublicˉkey(FilePath) {
    const Key = crypto.createPublicKey(
        Readˉordinaryˉfile(FilePath, "Public key", MAX_KEY_BYTES),
    );
    return Publicˉinformation(Key);
}

function Signingˉmessage(Kind, Message) {
    return Buffer.concat([
        DOMAIN_PREFIX,
        Buffer.from(`${Kind}\0`, "ascii"),
        Message,
    ]);
}

function Signatureˉtext(Kind, KeyId, Message, PrivateKey) {
    const Signature = crypto.sign(null, Signingˉmessage(Kind, Message), PrivateKey);
    if (Signature.length !== 64) Fail("Ed25519 returned an unexpected signature size.");
    return Buffer.from(
        "windvale-signature 1\n" +
        `kind ${Kind}\n` +
        "algorithm ed25519\n" +
        `key ${KeyId}\n` +
        `message-sha256 ${Sha256(Message)}\n` +
        `signature ${Signature.toString("base64")}\n`,
        "utf8",
    );
}

function Parseˉsignature(Bytes, ExpectedKind, ExpectedKeyId, Message, PublicKey) {
    const Text = Bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Bytes) || Text.includes("\r")) {
        Fail("Signature metadata is not canonical UTF-8.");
    }
    const Lines = Text.endsWith("\n") ? Text.slice(0, -1).split("\n") : [];
    if (Lines.length !== 6 || Lines[0] !== "windvale-signature 1" ||
        Lines[1] !== `kind ${ExpectedKind}` || Lines[2] !== "algorithm ed25519" ||
        Lines[3] !== `key ${ExpectedKeyId}` ||
        Lines[4] !== `message-sha256 ${Sha256(Message)}` ||
        !Lines[5].startsWith("signature ")) {
        Fail("Signature metadata differs.");
    }
    const Encoded = Lines[5].slice("signature ".length);
    const Signature = Buffer.from(Encoded, "base64");
    if (Signature.length !== 64 || Signature.toString("base64") !== Encoded ||
        !crypto.verify(null, Signingˉmessage(ExpectedKind, Message), PublicKey, Signature)) {
        Fail(`Invalid ${ExpectedKind} signature.`);
    }
}

function Parseˉrootˉpolicy(PolicyBytes, SignatureBytes) {
    const Text = PolicyBytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(PolicyBytes) || Text.includes("\r")) {
        Fail("Root policy is not canonical UTF-8.");
    }
    const Lines = Text.endsWith("\n") ? Text.slice(0, -1).split("\n") : [];
    if (Lines.length !== 8 || Lines[0] !== "windvale-release-root 1" ||
        Lines[2] !== "algorithm ed25519") {
        Fail("Root policy structure differs.");
    }
    const GenerationMatch = /^policy-generation ([1-9][0-9]*)$/.exec(Lines[1]);
    const RootMatch = /^root-key ([0-9a-f]{64}) ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[3]);
    const ReleaseMatch = /^release-key ([0-9a-f]{64}) ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[4]);
    const PrefixMatch = /^release-version-prefix (0\.[0-9]+\.)$/.exec(Lines[5]);
    const MinimumMatch = /^release-sequence-min ([1-9][0-9]*)$/.exec(Lines[6]);
    const MaximumMatch = /^release-sequence-max ([1-9][0-9]*)$/.exec(Lines[7]);
    if (!GenerationMatch || !RootMatch || !ReleaseMatch || !PrefixMatch ||
        !MinimumMatch || !MaximumMatch) {
        Fail("Root policy record differs.");
    }
    const RootDer = Buffer.from(RootMatch[2], "base64");
    const ReleaseDer = Buffer.from(ReleaseMatch[2], "base64");
    if (RootDer.toString("base64") !== RootMatch[2] ||
        ReleaseDer.toString("base64") !== ReleaseMatch[2] ||
        Sha256(RootDer) !== RootMatch[1] || Sha256(ReleaseDer) !== ReleaseMatch[1]) {
        Fail("Root policy key identity differs.");
    }
    const RootKey = Publicˉinformation(crypto.createPublicKey({
        key: RootDer,
        format: "der",
        type: "spki",
    }));
    const ReleaseKey = Publicˉinformation(crypto.createPublicKey({
        key: ReleaseDer,
        format: "der",
        type: "spki",
    }));
    Parseˉsignature(SignatureBytes, "root-policy", RootKey.id, PolicyBytes, RootKey.key);
    const Minimum = Parseˉdecimal(MinimumMatch[1], "minimum release sequence");
    const Maximum = Parseˉdecimal(MaximumMatch[1], "maximum release sequence");
    if (Minimum > Maximum) Fail("Root policy sequence range is reversed.");
    return {
        generation: Parseˉdecimal(GenerationMatch[1], "policy generation"),
        versionPrefix: PrefixMatch[1],
        minimum: Minimum,
        maximum: Maximum,
        rootKey: RootKey,
        releaseKey: ReleaseKey,
    };
}

function Assertˉreleaseˉprofile(Artifacts) {
    const Observed = new Set(Artifacts.map(Artifact => `${Artifact.role}|${Artifact.target}`));
    for (const Required of REQUIRED_PROFILE) {
        if (!Observed.has(Required)) Fail(`Release profile is missing ${Required}.`);
    }
}

function Generateˉkey(Role, OutputPath) {
    if (!KEY_NAME.test(Role)) Fail(`Invalid key role: ${Role}`);
    const Output = Assertˉemptyˉdirectory(OutputPath, "Key output");
    process.stdout.write(`release key step=generate role=${Role} item=1/2\n`);
    const Pair = crypto.generateKeyPairSync("ed25519");
    const Public = Publicˉinformation(Pair.publicKey);
    const PrivatePem = Pair.privateKey.export({ type: "pkcs8", format: "pem" });
    const PublicPem = Pair.publicKey.export({ type: "spki", format: "pem" });
    fs.writeFileSync(path.join(Output, `${Role}-private.pem`), PrivatePem, {
        flag: "wx",
        mode: 0o600,
    });
    fs.writeFileSync(path.join(Output, `${Role}-public.pem`), PublicPem, {
        flag: "wx",
        mode: 0o644,
    });
    fs.writeFileSync(path.join(Output, `${Role}-key-id.txt`), `${Public.id}\n`, {
        flag: "wx",
        mode: 0o644,
    });
    process.stdout.write(`release key step=report role=${Role} item=2/2 key=${Public.id}\n`);
    process.stdout.write(`release key status=Created role=${Role} files=3\n`);
}

function Createˉroot(InputPath, RootPrivatePath, ReleasePublicPath, OutputPath) {
    const Input = JSON.parse(Readˉcanonicalˉtext(
        InputPath,
        "Root policy input",
        MAX_ROOT_INPUT_BYTES,
    ));
    Assertˉexactˉkeys(Input, [
        "format",
        "policyGeneration",
        "versionPrefix",
        "minimumSequence",
        "maximumSequence",
    ], "Root policy input");
    if (Input.format !== "windvale-release-root-input-1" ||
        typeof Input.versionPrefix !== "string" ||
        !/^0\.[0-9]+\.$/.test(Input.versionPrefix)) {
        Fail("Root policy input header differs.");
    }
    const Generation = Parseˉpositiveˉinteger(Input.policyGeneration, "policy generation");
    const Minimum = Parseˉpositiveˉinteger(Input.minimumSequence, "minimum release sequence");
    const Maximum = Parseˉpositiveˉinteger(Input.maximumSequence, "maximum release sequence");
    if (Minimum > Maximum) Fail("Root policy sequence range is reversed.");
    const RootPrivate = Readˉprivateˉkey(RootPrivatePath);
    const RootPublic = Publicˉinformation(RootPrivate);
    const ReleasePublic = Readˉpublicˉkey(ReleasePublicPath);
    if (RootPublic.id === ReleasePublic.id) Fail("Root and release keys must differ.");
    const Output = Assertˉemptyˉdirectory(OutputPath, "Root policy output");
    process.stdout.write("release root step=construct item=1/3\n");
    const Policy = Buffer.from(
        "windvale-release-root 1\n" +
        `policy-generation ${Generation}\n` +
        "algorithm ed25519\n" +
        `root-key ${RootPublic.id} ${RootPublic.der.toString("base64")}\n` +
        `release-key ${ReleasePublic.id} ${ReleasePublic.der.toString("base64")}\n` +
        `release-version-prefix ${Input.versionPrefix}\n` +
        `release-sequence-min ${Minimum}\n` +
        `release-sequence-max ${Maximum}\n`,
        "utf8",
    );
    const Signature = Signatureˉtext("root-policy", RootPublic.id, Policy, RootPrivate);
    process.stdout.write("release root step=write item=2/3\n");
    fs.writeFileSync(path.join(Output, "Root-Policy.txt"), Policy, { flag: "wx", mode: 0o644 });
    fs.writeFileSync(path.join(Output, "Root-Policy.sig"), Signature, { flag: "wx", mode: 0o644 });
    process.stdout.write("release root step=report item=3/3\n");
    process.stdout.write(
        `release root status=Created generation=${Generation} root-key=${RootPublic.id} ` +
        `release-key=${ReleasePublic.id} policy=${Sha256(Policy)}\n`,
    );
}

function Createˉrelease(PolicyPath, ReleasePrivatePath, InputPath, SourcePath, OutputPath) {
    const PolicyDirectory = Assertˉordinaryˉdirectory(PolicyPath, "Root policy directory");
    const PolicyBytes = Readˉordinaryˉfile(
        path.join(PolicyDirectory, "Root-Policy.txt"),
        "Root policy",
        MAX_ROOT_POLICY_BYTES,
    );
    const PolicySignature = Readˉordinaryˉfile(
        path.join(PolicyDirectory, "Root-Policy.sig"),
        "Root signature",
        MAX_SIGNATURE_BYTES,
    );
    const Policy = Parseˉrootˉpolicy(PolicyBytes, PolicySignature);
    const ReleasePrivate = Readˉprivateˉkey(ReleasePrivatePath);
    const ReleasePublic = Publicˉinformation(ReleasePrivate);
    if (ReleasePublic.id !== Policy.releaseKey.id) {
        Fail("The private release key is not delegated by the root policy.");
    }
    const Input = JSON.parse(Readˉcanonicalˉtext(
        InputPath,
        "Release input",
        MAX_RELEASE_INPUT_BYTES,
    ));
    Assertˉexactˉkeys(Input, [
        "format",
        "version",
        "channel",
        "sequence",
        "revision",
        "tree",
        "artifacts",
    ], "Release input");
    if (Input.format !== "windvale-release-envelope-input-1" ||
        !VERSION.test(Input.version ?? "") || Input.channel !== "preview" ||
        !GIT_ID.test(Input.revision ?? "") || !GIT_ID.test(Input.tree ?? "") ||
        !Array.isArray(Input.artifacts) || Input.artifacts.length < REQUIRED_PROFILE.size ||
        Input.artifacts.length > 4096) {
        Fail("Release input header differs.");
    }
    const Sequence = Parseˉpositiveˉinteger(Input.sequence, "release sequence");
    if (!Input.version.startsWith(Policy.versionPrefix) ||
        Sequence < Policy.minimum || Sequence > Policy.maximum) {
        Fail("Release input is outside the root policy.");
    }
    const SourceRoot = Assertˉordinaryˉdirectory(SourcePath, "Artifact source");
    const Output = Assertˉemptyˉdirectory(OutputPath, "Release output");
    process.stdout.write("release envelope step=admit-inputs item=1/4\n");
    const Artifacts = [];
    let TotalBytes = 0;
    for (const Declaration of Input.artifacts) {
        Assertˉexactˉkeys(Declaration, [
            "role",
            "target",
            "source",
            "path",
            "bytes",
            "sha256",
        ], "Release artifact");
        if (!TOKEN.test(Declaration.role ?? "") || !TOKEN.test(Declaration.target ?? "") ||
            Buffer.byteLength(Declaration.role, "utf8") > MAX_TOKEN_BYTES ||
            Buffer.byteLength(Declaration.target, "utf8") > MAX_TOKEN_BYTES ||
            !SHA256.test(Declaration.sha256 ?? "")) {
            Fail("Release artifact identity fields differ.");
        }
        const SourceRelative = Assertˉordinaryˉrelativeˉpath(Declaration.source, "artifact source path");
        const ReleasePath = Assertˉordinaryˉrelativeˉpath(Declaration.path, "artifact release path");
        if (!ReleasePath.startsWith("Artifacts/")) {
            Fail("Release artifact paths must be below Artifacts/.");
        }
        const ExpectedBytes = Parseˉpositiveˉinteger(
            Declaration.bytes,
            "artifact byte length",
            MAX_ARTIFACT_BYTES,
        );
        const Candidate = path.resolve(SourceRoot, ...SourceRelative.split("/"));
        const RealCandidate = fs.realpathSync(Candidate);
        if (!RealCandidate.startsWith(`${SourceRoot}${path.sep}`)) {
            Fail("An artifact source escapes its root.");
        }
        const Bytes = Readˉordinaryˉfile(
            Candidate,
            "Release artifact",
            MAX_ARTIFACT_BYTES,
        );
        if (Bytes.length !== ExpectedBytes || Sha256(Bytes) !== Declaration.sha256) {
            Fail(`Release artifact identity changed: ${SourceRelative}`);
        }
        TotalBytes += Bytes.length;
        if (!Number.isSafeInteger(TotalBytes) || TotalBytes > MAX_RELEASE_BYTES) {
            Fail("Release artifact bytes exceed the bounded profile.");
        }
        Artifacts.push({
            role: Declaration.role,
            target: Declaration.target,
            path: ReleasePath,
            source: Candidate,
            value: Bytes,
            bytes: Bytes.length,
            sha256: Declaration.sha256,
        });
    }
    Artifacts.sort((Left, Right) => Buffer.from(
        `${Left.role}\0${Left.target}\0${Left.path}`,
    ).compare(Buffer.from(`${Right.role}\0${Right.target}\0${Right.path}`)));
    const CaseFoldedPaths = new Set();
    for (let Index = 1; Index < Artifacts.length; Index++) {
        const Previous = Artifacts[Index - 1];
        const Current = Artifacts[Index];
        if (Previous.path === Current.path ||
            (Previous.role === Current.role && Previous.target === Current.target)) {
            Fail("Release artifacts contain a duplicate path or role/target pair.");
        }
    }
    for (const Artifact of Artifacts) {
        const Folded = Artifact.path.toLowerCase();
        if (CaseFoldedPaths.has(Folded)) {
            Fail("Release artifacts contain a cross-host case-colliding path.");
        }
        CaseFoldedPaths.add(Folded);
    }
    Assertˉreleaseˉprofile(Artifacts);
    const OutputEntries = new Set();
    for (const Artifact of Artifacts) {
        const Parts = Artifact.path.split("/");
        for (let Length = 1; Length < Parts.length; Length++) {
            OutputEntries.add(Parts.slice(0, Length).join("/"));
        }
        OutputEntries.add(Artifact.path);
    }
    if (OutputEntries.size + 4 > MAX_INVENTORY_ENTRIES) {
        Fail("Release directory inventory exceeds the bounded profile.");
    }
    const Lines = [
        "windvale-release-manifest 1",
        `version ${Input.version}`,
        `channel ${Input.channel}`,
        `sequence ${Sequence}`,
        `revision ${Input.revision}`,
        `tree ${Input.tree}`,
        `root-policy-sha256 ${Sha256(PolicyBytes)}`,
        `release-key ${ReleasePublic.id}`,
        `artifact-count ${Artifacts.length}`,
    ];
    for (const Artifact of Artifacts) {
        Lines.push(
            `artifact ${Artifact.role} ${Artifact.target} ${Artifact.sha256} ` +
            `${Artifact.bytes} ${Artifact.path}`,
        );
    }
    const Manifest = Buffer.from(`${Lines.join("\n")}\n`, "utf8");
    const ManifestSignature = Signatureˉtext(
        "release-manifest",
        ReleasePublic.id,
        Manifest,
        ReleasePrivate,
    );
    process.stdout.write("release envelope step=write-metadata item=2/4\n");
    fs.writeFileSync(path.join(Output, "Root-Policy.txt"), PolicyBytes, { flag: "wx", mode: 0o644 });
    fs.writeFileSync(path.join(Output, "Root-Policy.sig"), PolicySignature, { flag: "wx", mode: 0o644 });
    fs.writeFileSync(path.join(Output, "Release-Manifest.txt"), Manifest, { flag: "wx", mode: 0o644 });
    fs.writeFileSync(path.join(Output, "Release-Manifest.sig"), ManifestSignature, { flag: "wx", mode: 0o644 });
    process.stdout.write(`release envelope step=copy-artifacts item=3/4 count=${Artifacts.length}\n`);
    for (const Artifact of Artifacts) {
        const Destination = path.join(Output, ...Artifact.path.split("/"));
        fs.mkdirSync(path.dirname(Destination), { recursive: true });
        fs.writeFileSync(Destination, Artifact.value, { flag: "wx", mode: 0o644 });
    }
    process.stdout.write("release envelope step=report item=4/4\n");
    process.stdout.write(
        `release envelope status=Created version=${Input.version} sequence=${Sequence} ` +
        `artifacts=${Artifacts.length} bytes=${TotalBytes} manifest=${Sha256(Manifest)}\n`,
    );
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "generate-key" && Arguments.length === 2) {
        Generateˉkey(Arguments[0], Arguments[1]);
    } else if (Command === "create-root" && Arguments.length === 4) {
        Createˉroot(Arguments[0], Arguments[1], Arguments[2], Arguments[3]);
    } else if (Command === "create-release" && Arguments.length === 5) {
        Createˉrelease(Arguments[0], Arguments[1], Arguments[2], Arguments[3], Arguments[4]);
    } else {
        process.stderr.write(
            "Usage: node Create-Release-Envelope.mjs " +
            "<generate-key root|release output-directory|" +
            "create-root input root-private-key release-public-key output-directory|" +
            "create-release policy-directory release-private-key input source-directory output-directory>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
