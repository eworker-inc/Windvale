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
const MIN_PASSPHRASE_BYTES = 16;
const MAX_PASSPHRASE_BYTES = 1_024;
const SCRYPT_N = 131_072;
const SCRYPT_R = 8;
const SCRYPT_P = 1;
const SCRYPT_MAX_MEMORY = 268_435_456;
const DOMAIN_PREFIX = Buffer.from("windvale-release-signature-v1\0", "ascii");
const ENCRYPTED_KEY_HEADER = "windvale-encrypted-private-key 1";
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

function Decodeˉcanonicalˉbase64(Text, Description, ExpectedBytes) {
    if (!/^[A-Za-z0-9+/]+={0,2}$/.test(Text)) Fail(`Invalid ${Description}.`);
    const Bytes = Buffer.from(Text, "base64");
    if (Bytes.length !== ExpectedBytes || Bytes.toString("base64") !== Text) {
        Fail(`Invalid ${Description}.`);
    }
    return Bytes;
}

function Validateˉpassphrase(Passphrase) {
    const Text = Passphrase.toString("utf8");
    if (Passphrase.length < MIN_PASSPHRASE_BYTES ||
        Passphrase.length > MAX_PASSPHRASE_BYTES ||
        !Buffer.from(Text, "utf8").equals(Passphrase) ||
        Text.includes("\0") || Text.includes("\r") || Text.includes("\n")) {
        Fail(
            `A key passphrase must be ${MIN_PASSPHRASE_BYTES}..` +
            `${MAX_PASSPHRASE_BYTES} bytes of UTF-8 without control separators.`,
        );
    }
    return Passphrase;
}

function Readˉmaskedˉpassphrase(Prompt) {
    if (!process.stdin.isTTY || typeof process.stdin.setRawMode !== "function") {
        Fail("Masked passphrase input requires a terminal.");
    }
    const Storage = Buffer.alloc(MAX_PASSPHRASE_BYTES);
    let Length = 0;
    const Byte = Buffer.alloc(1);
    let Passphrase;
    process.stdout.write(Prompt);
    process.stdin.setRawMode(true);
    try {
        for (;;) {
            if (fs.readSync(process.stdin.fd, Byte, 0, 1, null) !== 1) {
                Fail("Passphrase input ended unexpectedly.");
            }
            const Value = Byte[0];
            if (Value === 3) {
                process.stdout.write("\n");
                Fail("Passphrase input was cancelled.");
            }
            if (Value === 10 || Value === 13) {
                process.stdout.write("\n");
                break;
            }
            if (Value === 8 || Value === 127) {
                if (Length > 0) {
                    Storage[--Length] = 0;
                    process.stdout.write("\b \b");
                }
                continue;
            }
            if (Value < 32) Fail("Unsupported control byte in passphrase input.");
            if (Length >= MAX_PASSPHRASE_BYTES) {
                Fail("The key passphrase exceeds its byte limit.");
            }
            Storage[Length++] = Value;
            process.stdout.write("*");
        }
        Passphrase = Buffer.from(Storage.subarray(0, Length));
    } finally {
        process.stdin.setRawMode(false);
        Byte.fill(0);
        Storage.fill(0);
    }
    try {
        return Validateˉpassphrase(Passphrase);
    } catch (ErrorValue) {
        Passphrase.fill(0);
        throw ErrorValue;
    }
}

function Readˉkeyˉpassphrase(Confirm) {
    if (process.stdin.isTTY) {
        const First = Readˉmaskedˉpassphrase("Key passphrase: ");
        if (!Confirm) return First;
        const Second = Readˉmaskedˉpassphrase("Confirm key passphrase: ");
        const Matches = First.length === Second.length &&
            crypto.timingSafeEqual(First, Second);
        Second.fill(0);
        if (!Matches) {
            First.fill(0);
            Fail("The key passphrases do not match.");
        }
        return First;
    }
    const Input = fs.readFileSync(process.stdin.fd);
    const Text = Input.toString("utf8").replaceAll("\r\n", "\n");
    Input.fill(0);
    if (Text.includes("\r") || !Text.endsWith("\n")) {
        Fail("Piped key passphrase input must contain complete lines.");
    }
    const Lines = Text.slice(0, -1).split("\n");
    if (Lines.length !== (Confirm ? 2 : 1)) {
        Fail("Piped key passphrase input has the wrong line count.");
    }
    const First = Validateˉpassphrase(Buffer.from(Lines[0], "utf8"));
    if (!Confirm) return First;
    const Second = Validateˉpassphrase(Buffer.from(Lines[1], "utf8"));
    const Matches = First.length === Second.length && crypto.timingSafeEqual(First, Second);
    Second.fill(0);
    if (!Matches) {
        First.fill(0);
        Fail("The key passphrases do not match.");
    }
    return First;
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

function Deriveˉkey(Passphrase, Salt) {
    return crypto.scryptSync(Passphrase, Salt, 32, {
        N: SCRYPT_N,
        r: SCRYPT_R,
        p: SCRYPT_P,
        maxmem: SCRYPT_MAX_MEMORY,
    });
}

function Encryptˉprivateˉkey(Role, PrivateKey, PublicId, Passphrase) {
    const Salt = crypto.randomBytes(32);
    const Nonce = crypto.randomBytes(12);
    const PrivateDer = PrivateKey.export({ type: "pkcs8", format: "der" });
    const Header = Buffer.from(
        `${ENCRYPTED_KEY_HEADER}\n` +
        `role ${Role}\n` +
        `public-key ${PublicId}\n` +
        "kdf scrypt\n" +
        `scrypt-n ${SCRYPT_N}\n` +
        `scrypt-r ${SCRYPT_R}\n` +
        `scrypt-p ${SCRYPT_P}\n` +
        `salt ${Salt.toString("base64")}\n` +
        "cipher aes-256-gcm\n" +
        `nonce ${Nonce.toString("base64")}\n` +
        `pkcs8-bytes ${PrivateDer.length}\n`,
        "utf8",
    );
    let Derived;
    try {
        Derived = Deriveˉkey(Passphrase, Salt);
        const Cipher = crypto.createCipheriv("aes-256-gcm", Derived, Nonce, {
            authTagLength: 16,
        });
        Cipher.setAAD(Header);
        const Ciphertext = Buffer.concat([Cipher.update(PrivateDer), Cipher.final()]);
        const Tag = Cipher.getAuthTag();
        return Buffer.concat([
            Header,
            Buffer.from(
                `ciphertext ${Ciphertext.toString("base64")}\n` +
                `tag ${Tag.toString("base64")}\n`,
                "utf8",
            ),
        ]);
    } finally {
        if (Derived) Derived.fill(0);
        PrivateDer.fill(0);
        Salt.fill(0);
        Nonce.fill(0);
    }
}

function Readˉencryptedˉprivateˉkey(Bytes, ExpectedRole, Passphrase) {
    const Text = Bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Bytes) || Text.includes("\r")) {
        Fail("Encrypted private key is not canonical UTF-8.");
    }
    const Lines = Text.endsWith("\n") ? Text.slice(0, -1).split("\n") : [];
    if (Lines.length !== 13 || Lines[0] !== ENCRYPTED_KEY_HEADER ||
        Lines[1] !== `role ${ExpectedRole}` || Lines[3] !== "kdf scrypt" ||
        Lines[4] !== `scrypt-n ${SCRYPT_N}` || Lines[5] !== `scrypt-r ${SCRYPT_R}` ||
        Lines[6] !== `scrypt-p ${SCRYPT_P}` || Lines[8] !== "cipher aes-256-gcm") {
        Fail("Encrypted private key structure differs.");
    }
    const PublicMatch = /^public-key ([0-9a-f]{64})$/.exec(Lines[2]);
    const SaltMatch = /^salt ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[7]);
    const NonceMatch = /^nonce ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[9]);
    const BytesMatch = /^pkcs8-bytes ([1-9][0-9]*)$/.exec(Lines[10]);
    const CiphertextMatch = /^ciphertext ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[11]);
    const TagMatch = /^tag ([A-Za-z0-9+/]+={0,2})$/.exec(Lines[12]);
    if (!PublicMatch || !SaltMatch || !NonceMatch || !BytesMatch ||
        !CiphertextMatch || !TagMatch) {
        Fail("Encrypted private key records differ.");
    }
    const PlainBytes = Parseˉdecimal(BytesMatch[1], "private PKCS #8 byte length", MAX_KEY_BYTES);
    const Salt = Decodeˉcanonicalˉbase64(SaltMatch[1], "private-key salt", 32);
    const Nonce = Decodeˉcanonicalˉbase64(NonceMatch[1], "private-key nonce", 12);
    const Ciphertext = Decodeˉcanonicalˉbase64(
        CiphertextMatch[1],
        "private-key ciphertext",
        PlainBytes,
    );
    const Tag = Decodeˉcanonicalˉbase64(TagMatch[1], "private-key authentication tag", 16);
    const Header = Buffer.from(`${Lines.slice(0, 11).join("\n")}\n`, "utf8");
    let Derived;
    let Plaintext;
    try {
        Derived = Deriveˉkey(Passphrase, Salt);
        const Decipher = crypto.createDecipheriv("aes-256-gcm", Derived, Nonce, {
            authTagLength: 16,
        });
        Decipher.setAAD(Header);
        Decipher.setAuthTag(Tag);
        Plaintext = Buffer.concat([Decipher.update(Ciphertext), Decipher.final()]);
        const Key = crypto.createPrivateKey({ key: Plaintext, format: "der", type: "pkcs8" });
        if (Key.type !== "private" || Key.asymmetricKeyType !== "ed25519" ||
            Publicˉinformation(Key).id !== PublicMatch[1]) {
            Fail("Encrypted private key identity differs.");
        }
        return Key;
    } catch {
        Fail("Encrypted private key could not be unlocked.");
    } finally {
        if (Derived) Derived.fill(0);
        Salt.fill(0);
        Nonce.fill(0);
        Ciphertext.fill(0);
        Tag.fill(0);
        if (Plaintext) Plaintext.fill(0);
    }
}

function Readˉprivateˉkey(FilePath, ExpectedRole, Passphrase) {
    const Bytes = Readˉordinaryˉfile(FilePath, "Private key", MAX_KEY_BYTES);
    if (Bytes.toString("utf8").startsWith(`${ENCRYPTED_KEY_HEADER}\n`)) {
        if (!Passphrase) Fail("An encrypted private key requires --key-passphrase.");
        return Readˉencryptedˉprivateˉkey(Bytes, ExpectedRole, Passphrase);
    }
    if (Passphrase) Fail("--key-passphrase requires a Windvale encrypted private key.");
    const Key = crypto.createPrivateKey(Bytes);
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

function Generateˉkey(Role, OutputPath, Passphrase) {
    if (!KEY_NAME.test(Role)) Fail(`Invalid key role: ${Role}`);
    const Output = Assertˉemptyˉdirectory(OutputPath, "Key output");
    process.stdout.write(`release key step=generate role=${Role} item=1/2\n`);
    const Pair = crypto.generateKeyPairSync("ed25519");
    const Public = Publicˉinformation(Pair.publicKey);
    const PrivateBytes = Passphrase ?
        Encryptˉprivateˉkey(Role, Pair.privateKey, Public.id, Passphrase) :
        Pair.privateKey.export({ type: "pkcs8", format: "pem" });
    const PrivateName = Passphrase ? `${Role}-private.wvkey` : `${Role}-private.pem`;
    const PublicPem = Pair.publicKey.export({ type: "spki", format: "pem" });
    fs.writeFileSync(path.join(Output, PrivateName), PrivateBytes, {
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
    process.stdout.write(
        `release key status=Created role=${Role} files=3 protection=` +
        `${Passphrase ? "Passphrase" : "UnencryptedTest"}\n`,
    );
}

function Createˉroot(InputPath, RootPrivatePath, ReleasePublicPath, OutputPath, Passphrase) {
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
    const RootPrivate = Readˉprivateˉkey(RootPrivatePath, "root", Passphrase);
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

function Createˉrelease(
    PolicyPath,
    ReleasePrivatePath,
    InputPath,
    SourcePath,
    OutputPath,
    Passphrase,
) {
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
    const ReleasePrivate = Readˉprivateˉkey(ReleasePrivatePath, "release", Passphrase);
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
    if (Command === "generate-test-key" && Arguments.length === 2) {
        Generateˉkey(Arguments[0], Arguments[1]);
    } else if (Command === "generate-key" && Arguments.length === 3 &&
        Arguments[2] === "--key-passphrase") {
        const Passphrase = Readˉkeyˉpassphrase(true);
        try {
            Generateˉkey(Arguments[0], Arguments[1], Passphrase);
        } finally {
            Passphrase.fill(0);
        }
    } else if (Command === "create-root" && Arguments.length === 4) {
        Createˉroot(Arguments[0], Arguments[1], Arguments[2], Arguments[3]);
    } else if (Command === "create-root" && Arguments.length === 5 &&
        Arguments[4] === "--key-passphrase") {
        const Passphrase = Readˉkeyˉpassphrase(false);
        try {
            Createˉroot(Arguments[0], Arguments[1], Arguments[2], Arguments[3], Passphrase);
        } finally {
            Passphrase.fill(0);
        }
    } else if (Command === "create-release" && Arguments.length === 5) {
        Createˉrelease(Arguments[0], Arguments[1], Arguments[2], Arguments[3], Arguments[4]);
    } else if (Command === "create-release" && Arguments.length === 6 &&
        Arguments[5] === "--key-passphrase") {
        const Passphrase = Readˉkeyˉpassphrase(false);
        try {
            Createˉrelease(
                Arguments[0],
                Arguments[1],
                Arguments[2],
                Arguments[3],
                Arguments[4],
                Passphrase,
            );
        } finally {
            Passphrase.fill(0);
        }
    } else {
        process.stderr.write(
            "Usage: node Create-Release-Envelope.mjs " +
            "<generate-test-key root|release output-directory|" +
            "generate-key root|release output-directory --key-passphrase|" +
            "create-root input root-private-key release-public-key output-directory " +
            "[--key-passphrase]|create-release policy-directory release-private-key " +
            "input source-directory output-directory [--key-passphrase]>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
