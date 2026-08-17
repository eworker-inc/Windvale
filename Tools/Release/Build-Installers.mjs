import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, "../..");
const DEFAULT_INPUT_PATH = path.join(
    REPOSITORY_ROOT,
    "Distribution/Installers/Windvale-Development-Installer.json",
);
const SAFE_RELATIVE_PATH = /^[A-Za-z0-9._/-]+$/;
const SHA256 = /^[0-9a-f]{64}$/;
const VERSION = /^0\.[0-9]+\.[0-9]+(?:-dev\.[0-9]+)?$/;
const ARCHIVE_ROOT = /^windvale-[a-z0-9.-]+-(?:windows|linux)-x64$/;
const DOS_DATE_1980_01_01 = 0x0021;

function Sha256(Value) {
    return crypto.createHash("sha256").update(Value).digest("hex");
}

function Fail(Message) {
    throw new Error(Message);
}

function Assertˉordinaryˉrelativeˉpath(Value, Description) {
    if (typeof Value !== "string" || !SAFE_RELATIVE_PATH.test(Value) ||
        Value.startsWith("/") || Value.endsWith("/") || Value.includes("//") ||
        Value.split("/").includes("..")) {
        Fail(`Invalid ${Description}: ${Value}`);
    }
    return Value;
}

function Assertˉmode(Value) {
    if (!/^0[0-7]{3}$/.test(Value ?? "")) {
        Fail(`Invalid installer mode: ${Value}`);
    }
    return Number.parseInt(Value, 8);
}

function Readˉcanonicalˉtext(SourcePath) {
    const Bytes = fs.readFileSync(SourcePath);
    const Text = Bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Bytes)) {
        Fail(`Installer text is not canonical UTF-8: ${SourcePath}`);
    }
    const Normalized = Text.replaceAll("\r\n", "\n");
    if (Normalized.includes("\r") || Normalized.includes("\0")) {
        Fail(`Installer text contains an unsupported character: ${SourcePath}`);
    }
    return Normalized;
}

function Textˉbytes(Text, LineEnding = "lf") {
    if (Text.includes("\r")) {
        Fail("Generated installer text must use logical LF before encoding.");
    }
    const Encoded = LineEnding === "crlf" ? Text.replaceAll("\n", "\r\n") : Text;
    return Buffer.from(Encoded, "utf8");
}

function Readˉdeclaredˉfile(Declaration) {
    const Source = Assertˉordinaryˉrelativeˉpath(Declaration.source, "source path");
    const Destination = Assertˉordinaryˉrelativeˉpath(
        Declaration.path,
        "payload path",
    );
    const SourcePath = path.join(REPOSITORY_ROOT, ...Source.split("/"));
    let Bytes;
    if (Declaration.text === "lf") {
        Bytes = Textˉbytes(Readˉcanonicalˉtext(SourcePath));
    } else if (Declaration.text === undefined) {
        Bytes = fs.readFileSync(SourcePath);
    } else {
        Fail(`Unsupported installer text policy: ${Declaration.text}`);
    }
    if (!Number.isSafeInteger(Declaration.bytes) || Declaration.bytes < 0 ||
        Bytes.length !== Declaration.bytes || !SHA256.test(Declaration.sha256 ?? "") ||
        Sha256(Bytes) !== Declaration.sha256) {
        Fail(`Installer input identity changed: ${Source}`);
    }
    return {
        path: Destination,
        bytes: Bytes,
        mode: Assertˉmode(Declaration.mode),
    };
}

function Addˉpayloadˉfile(Files, Entry) {
    Assertˉordinaryˉrelativeˉpath(Entry.path, "generated payload path");
    if (!Buffer.isBuffer(Entry.bytes) || !Number.isInteger(Entry.mode)) {
        Fail(`Invalid generated payload entry: ${Entry.path}`);
    }
    if (Files.some(Current => Current.path === Entry.path)) {
        Fail(`Duplicate installer payload path: ${Entry.path}`);
    }
    Files.push(Entry);
}

function Readˉtemplate(Target, Name, LineEnding = "lf") {
    const TemplatePath = path.join(
        REPOSITORY_ROOT,
        "Distribution",
        "Installers",
        "Templates",
        Target,
        Name,
    );
    return Textˉbytes(Readˉcanonicalˉtext(TemplatePath), LineEnding);
}

function Replaceˉtokens(Bytes, Values, LineEnding = "lf") {
    let Text = Bytes.toString("utf8").replaceAll("\r\n", "\n");
    for (const [Name, Value] of Object.entries(Values)) {
        Text = Text.replaceAll(`@@${Name}@@`, Value);
    }
    if (/@@[A-Z0-9_]+@@/.test(Text)) {
        Fail("An installer template token remains unresolved.");
    }
    return Textˉbytes(Text, LineEnding);
}

function Versionˉtext(VersionValue, Channel, Target) {
    return Textˉbytes(
        `Windvale ${VersionValue}\n` +
        `Channel ${Channel}\n` +
        `Target ${Target}\n`,
    );
}

function Readmeˉtext(VersionValue, Channel, Target) {
    if (Channel === "stable") {
        return Textˉbytes(
            `Windvale ${VersionValue} installer (${Target})\n` +
            "\n" +
            "This is the installable Windvale v0.1.0 product preview.\n" +
            "Verify the archive through its signed release envelope before installation.\n" +
            "This preview carries no automatic update client.\n" +
            "\n" +
            "Installed commands:\n" +
            "  wv, wvbuild, wvasm, wvlink, wvrun, wvdump, wvverify, wvpublish\n" +
            "\n" +
            "Use `wv version`, `wv tools`, and `wv doctor` for local inspection.\n" +
            "The native tools remain separate commands so their full argument lists are preserved.\n",
        );
    }
    return Textˉbytes(
        `Windvale ${VersionValue} development installer (${Target})\n` +
        "\n" +
        "This unsigned development installer is an early Milestone 3 artifact.\n" +
        "It is not the Windvale v0.1.0 release and carries no automatic update client.\n" +
        "\n" +
        "Installed commands:\n" +
        "  wv, wvbuild, wvasm, wvlink, wvrun, wvdump, wvverify, wvpublish\n" +
        "\n" +
        "Use `wv version`, `wv tools`, and `wv doctor` for local inspection.\n" +
        "The native tools remain separate commands so their full argument lists are preserved.\n",
    );
}

function Payloadˉmanifest(VersionValue, Target, Files) {
    const Lines = [
        "windvale-installer-payload 1",
        `version ${VersionValue}`,
        `target ${Target}`,
    ];
    for (const File of [...Files].sort((Left, Right) =>
        Buffer.from(Left.path).compare(Buffer.from(Right.path)))) {
        Lines.push(
            `file ${Sha256(File.bytes)} ${File.bytes.length} ` +
            `${File.mode.toString(8).padStart(4, "0")} ${File.path}`,
        );
    }
    return Textˉbytes(`${Lines.join("\n")}\n`);
}

function Resolveˉinputˉpath(Value) {
    if (Value === undefined) return DEFAULT_INPUT_PATH;
    const Relative = Assertˉordinaryˉrelativeˉpath(Value, "installer input path");
    if (!Relative.startsWith("Distribution/Installers/") ||
        !Relative.endsWith(".json")) {
        Fail("The installer input must be repository distribution metadata.");
    }
    return path.join(REPOSITORY_ROOT, ...Relative.split("/"));
}

function Loadˉinput(InputPath) {
    const Input = JSON.parse(Readˉcanonicalˉtext(Resolveˉinputˉpath(InputPath)));
    const IsDevelopment = Input.channel === "development" &&
        /-dev\.[0-9]+$/.test(Input.version ?? "");
    const IsStable = Input.channel === "stable" &&
        !String(Input.version ?? "").includes("-");
    if (Input.format !== "windvale-installer-input-1" ||
        !VERSION.test(Input.version ?? "") || (!IsDevelopment && !IsStable) ||
        !Array.isArray(Input.sharedFiles) || !Array.isArray(Input.targets) ||
        Input.targets.length !== 2) {
        Fail("The installer input envelope is invalid.");
    }
    const TargetIds = Input.targets.map(Target => Target.id).sort();
    if (TargetIds.join("|") !== "linux-x64|windows-x64") {
        Fail("The installer target set is invalid.");
    }
    return Input;
}

function Buildˉtarget(Input, Target) {
    if (!ARCHIVE_ROOT.test(`windvale-${Input.version}-${Target.id}`) ||
        !Array.isArray(Target.files) || Target.files.length !== 7 ||
        !Assertˉordinaryˉrelativeˉpath(Target.archive, "archive name") ||
        !["zip-store-1", "tar-gzip-store-1"].includes(Target.archiveFormat)) {
        Fail(`Invalid installer target: ${Target.id}`);
    }

    const PayloadFiles = [];
    for (const Shared of Input.sharedFiles) {
        Addˉpayloadˉfile(PayloadFiles, Readˉdeclaredˉfile(Shared));
    }
    for (const File of Target.files) {
        Addˉpayloadˉfile(PayloadFiles, Readˉdeclaredˉfile(File));
    }
    Addˉpayloadˉfile(PayloadFiles, {
        path: "README.txt",
        bytes: Readmeˉtext(Input.version, Input.channel, Target.id),
        mode: 0o644,
    });
    Addˉpayloadˉfile(PayloadFiles, {
        path: "VERSION",
        bytes: Versionˉtext(Input.version, Input.channel, Target.id),
        mode: 0o644,
    });
    if (Target.id === "windows-x64") {
        Addˉpayloadˉfile(PayloadFiles, {
            path: "bin/wv.cmd",
            bytes: Readˉtemplate(Target.id, "wv.cmd", "crlf"),
            mode: 0o755,
        });
        Addˉpayloadˉfile(PayloadFiles, {
            path: "bin/wv-run.ps1",
            bytes: Readˉtemplate(Target.id, "wv-run.ps1"),
            mode: 0o755,
        });
        Addˉpayloadˉfile(PayloadFiles, {
            path: "bin/wv-verify-installation.ps1",
            bytes: Replaceˉtokens(
                Readˉtemplate(Target.id, "wv-verify-installation.ps1"),
                {
                    POWERSHELL_VERSION_PATTERN: Input.channel === "development" ?
                        "^version ([0-9]+\\.[0-9]+\\.[0-9]+-dev\\.[0-9]+)$" :
                        "^version ([0-9]+\\.[0-9]+\\.[0-9]+)$",
                },
            ),
            mode: 0o755,
        });
    } else {
        Addˉpayloadˉfile(PayloadFiles, {
            path: "bin/wv",
            bytes: Readˉtemplate(Target.id, "wv"),
            mode: 0o755,
        });
        Addˉpayloadˉfile(PayloadFiles, {
            path: "bin/wv-verify-installation",
            bytes: Replaceˉtokens(
                Readˉtemplate(Target.id, "wv-verify-installation"),
                {
                    GREP_VERSION_PATTERN: Input.channel === "development" ?
                        "^version [0-9]+\\.[0-9]+\\.[0-9]+-dev\\.[0-9]+$" :
                        "^version [0-9]+\\.[0-9]+\\.[0-9]+$",
                },
            ),
            mode: 0o755,
        });
    }

    const PayloadManifest = Payloadˉmanifest(Input.version, Target.id, PayloadFiles);
    const PayloadSha256 = Sha256(PayloadManifest);
    const Generation = `${Input.version}-${Target.id}-${PayloadSha256.slice(0, 12)}`;
    const Tokens = {
        VERSION: Input.version,
        TARGET: Target.id,
        PAYLOAD_SHA256: PayloadSha256,
        GENERATION: Generation,
        INSTALLATION_RECORD: Input.channel === "development" ?
            "windvale-development-installation 1" : "windvale-installation 1",
        INSTALLATION_DESCRIPTION: Input.channel === "development" ?
            "Windvale development installation" : "Windvale installation",
    };
    const ArchiveFiles = [...PayloadFiles, {
        path: "Payload-Manifest.txt",
        bytes: PayloadManifest,
        mode: 0o644,
    }];
    if (Target.id === "windows-x64") {
        ArchiveFiles.push({
            path: "Install-Windvale.ps1",
            bytes: Replaceˉtokens(Readˉtemplate(Target.id, "Install-Windvale.ps1"), Tokens),
            mode: 0o755,
        }, {
            path: "Uninstall-Windvale.ps1",
            bytes: Replaceˉtokens(Readˉtemplate(Target.id, "Uninstall-Windvale.ps1"), Tokens),
            mode: 0o755,
        });
    } else {
        ArchiveFiles.push({
            path: "install.sh",
            bytes: Replaceˉtokens(Readˉtemplate(Target.id, "install.sh"), Tokens),
            mode: 0o755,
        }, {
            path: "uninstall.sh",
            bytes: Replaceˉtokens(Readˉtemplate(Target.id, "uninstall.sh"), Tokens),
            mode: 0o755,
        });
    }
    ArchiveFiles.sort((Left, Right) =>
        Buffer.from(Left.path).compare(Buffer.from(Right.path)));
    const RootName = `windvale-${Input.version}-${Target.id}`;
    const RootedFiles = ArchiveFiles.map(File => ({
        ...File,
        path: `${RootName}/${File.path}`,
    }));
    const ArchiveBytes = Target.archiveFormat === "zip-store-1" ?
        Writeˉzip(RootedFiles) : Gzipˉstored(Writeˉtar(RootedFiles));
    const ArchiveSha256 = Sha256(ArchiveBytes);
    if (Target.expectedArchiveSha256 !== "pending" &&
        (Target.expectedArchiveSha256 !== ArchiveSha256 ||
         Target.expectedArchiveBytes !== ArchiveBytes.length ||
         Target.expectedPayloadSha256 !== PayloadSha256)) {
        Fail(`Pinned installer identity changed: ${Target.id}`);
    }
    return {
        target: Target,
        archiveBytes: ArchiveBytes,
        archiveSha256: ArchiveSha256,
        payloadSha256: PayloadSha256,
        generation: Generation,
        files: RootedFiles,
    };
}

const CRC32_TABLE = (() => {
    const Table = new Uint32Array(256);
    for (let Value = 0; Value < 256; Value++) {
        let Current = Value;
        for (let Bit = 0; Bit < 8; Bit++) {
            Current = (Current & 1) !== 0 ?
                (0xedb88320 ^ (Current >>> 1)) : (Current >>> 1);
        }
        Table[Value] = Current >>> 0;
    }
    return Table;
})();

function Crc32(Value) {
    let Crc = 0xffffffff;
    for (const Byte of Value) {
        Crc = CRC32_TABLE[(Crc ^ Byte) & 0xff] ^ (Crc >>> 8);
    }
    return (Crc ^ 0xffffffff) >>> 0;
}

function Writeˉzip(Files) {
    const LocalParts = [];
    const CentralParts = [];
    let LocalOffset = 0;
    for (const File of Files) {
        const Name = Buffer.from(File.path, "utf8");
        const Crc = Crc32(File.bytes);
        const Local = Buffer.alloc(30);
        Local.writeUInt32LE(0x04034b50, 0);
        Local.writeUInt16LE(20, 4);
        Local.writeUInt16LE(0x0800, 6);
        Local.writeUInt16LE(0, 8);
        Local.writeUInt16LE(0, 10);
        Local.writeUInt16LE(DOS_DATE_1980_01_01, 12);
        Local.writeUInt32LE(Crc, 14);
        Local.writeUInt32LE(File.bytes.length, 18);
        Local.writeUInt32LE(File.bytes.length, 22);
        Local.writeUInt16LE(Name.length, 26);
        Local.writeUInt16LE(0, 28);
        LocalParts.push(Local, Name, File.bytes);

        const Central = Buffer.alloc(46);
        Central.writeUInt32LE(0x02014b50, 0);
        Central.writeUInt16LE(0x0314, 4);
        Central.writeUInt16LE(20, 6);
        Central.writeUInt16LE(0x0800, 8);
        Central.writeUInt16LE(0, 10);
        Central.writeUInt16LE(0, 12);
        Central.writeUInt16LE(DOS_DATE_1980_01_01, 14);
        Central.writeUInt32LE(Crc, 16);
        Central.writeUInt32LE(File.bytes.length, 20);
        Central.writeUInt32LE(File.bytes.length, 24);
        Central.writeUInt16LE(Name.length, 28);
        Central.writeUInt16LE(0, 30);
        Central.writeUInt16LE(0, 32);
        Central.writeUInt16LE(0, 34);
        Central.writeUInt16LE(0, 36);
        Central.writeUInt32LE(((0o100000 | File.mode) << 16) >>> 0, 38);
        Central.writeUInt32LE(LocalOffset, 42);
        CentralParts.push(Central, Name);
        LocalOffset += Local.length + Name.length + File.bytes.length;
    }
    const CentralBytes = Buffer.concat(CentralParts);
    const End = Buffer.alloc(22);
    End.writeUInt32LE(0x06054b50, 0);
    End.writeUInt16LE(0, 4);
    End.writeUInt16LE(0, 6);
    End.writeUInt16LE(Files.length, 8);
    End.writeUInt16LE(Files.length, 10);
    End.writeUInt32LE(CentralBytes.length, 12);
    End.writeUInt32LE(LocalOffset, 16);
    End.writeUInt16LE(0, 20);
    return Buffer.concat([...LocalParts, CentralBytes, End]);
}

function Writeˉascii(BufferValue, Offset, Length, Text) {
    const Value = Buffer.from(Text, "ascii");
    if (Value.length > Length) {
        Fail("A deterministic tar field is too long.");
    }
    Value.copy(BufferValue, Offset);
}

function Writeˉoctal(BufferValue, Offset, Length, Value) {
    const Text = Value.toString(8).padStart(Length - 1, "0");
    if (Text.length !== Length - 1) {
        Fail("A deterministic tar integer is too large.");
    }
    Writeˉascii(BufferValue, Offset, Length - 1, Text);
    BufferValue[Offset + Length - 1] = 0;
}

function Writeˉtar(Files) {
    const Parts = [];
    for (const File of Files) {
        const Header = Buffer.alloc(512);
        Writeˉascii(Header, 0, 100, File.path);
        Writeˉoctal(Header, 100, 8, File.mode);
        Writeˉoctal(Header, 108, 8, 0);
        Writeˉoctal(Header, 116, 8, 0);
        Writeˉoctal(Header, 124, 12, File.bytes.length);
        Writeˉoctal(Header, 136, 12, 0);
        Header.fill(0x20, 148, 156);
        Header[156] = 0x30;
        Writeˉascii(Header, 257, 6, "ustar\0");
        Writeˉascii(Header, 263, 2, "00");
        let Checksum = 0;
        for (const Byte of Header) Checksum += Byte;
        Writeˉascii(Header, 148, 6, Checksum.toString(8).padStart(6, "0"));
        Header[154] = 0;
        Header[155] = 0x20;
        Parts.push(Header, File.bytes);
        const Padding = (512 - (File.bytes.length % 512)) % 512;
        if (Padding !== 0) Parts.push(Buffer.alloc(Padding));
    }
    Parts.push(Buffer.alloc(1024));
    return Buffer.concat(Parts);
}

function Gzipˉstored(Value) {
    const Parts = [Buffer.from([0x1f, 0x8b, 0x08, 0x00, 0, 0, 0, 0, 0, 0xff])];
    for (let Offset = 0; Offset < Value.length || Offset === 0;) {
        const Remaining = Value.length - Offset;
        const Length = Math.min(Remaining, 65535);
        const Final = Offset + Length === Value.length;
        const Header = Buffer.alloc(5);
        Header[0] = Final ? 1 : 0;
        Header.writeUInt16LE(Length, 1);
        Header.writeUInt16LE((~Length) & 0xffff, 3);
        Parts.push(Header, Value.subarray(Offset, Offset + Length));
        Offset += Length;
        if (Final) break;
    }
    const Trailer = Buffer.alloc(8);
    Trailer.writeUInt32LE(Crc32(Value), 0);
    Trailer.writeUInt32LE(Value.length >>> 0, 4);
    Parts.push(Trailer);
    return Buffer.concat(Parts);
}

function Findˉtarget(Input, ArtifactPath) {
    const Name = path.basename(ArtifactPath);
    const Target = Input.targets.find(Current => Current.archive === Name);
    if (!Target) Fail(`Unknown installer artifact: ${Name}`);
    return Target;
}

function Verifyˉartifact(Input, ArtifactPath) {
    const Target = Findˉtarget(Input, ArtifactPath);
    const Built = Buildˉtarget(Input, Target);
    const Observed = fs.readFileSync(ArtifactPath);
    if (!Observed.equals(Built.archiveBytes)) {
        Fail(`Installer artifact differs: ${Target.id}`);
    }
    return Built;
}

function Assertˉemptyˉoutputˉdirectory(OutputPath) {
    const Stat = fs.lstatSync(OutputPath);
    if (!Stat.isDirectory() || Stat.isSymbolicLink() || fs.readdirSync(OutputPath).length !== 0) {
        Fail("The installer output must be an existing empty ordinary directory.");
    }
    return fs.realpathSync(OutputPath);
}

function Extractˉverified(Built, OutputPath) {
    const Root = Assertˉemptyˉoutputˉdirectory(OutputPath);
    for (const File of Built.files) {
        Assertˉordinaryˉrelativeˉpath(File.path, "archive extraction path");
        const Destination = path.resolve(Root, ...File.path.split("/"));
        if (!Destination.startsWith(`${Root}${path.sep}`)) {
            Fail("A verified installer path escapes its extraction root.");
        }
        fs.mkdirSync(path.dirname(Destination), { recursive: true });
        fs.writeFileSync(Destination, File.bytes, { flag: "wx", mode: File.mode });
        fs.chmodSync(Destination, File.mode);
    }
}

function Printˉidentity(Built) {
    process.stdout.write(
        `installer artifact target=${Built.target.id} ` +
        `bytes=${Built.archiveBytes.length} sha256=${Built.archiveSha256} ` +
        `payload=${Built.payloadSha256} generation=${Built.generation}\n`,
    );
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "build" && (Arguments.length === 1 || Arguments.length === 2)) {
        process.stdout.write("installer step=load-input item=1/4\n");
        const Input = Loadˉinput(Arguments[1]);
        const OutputRoot = Assertˉemptyˉoutputˉdirectory(Arguments[0]);
        const BuiltTargets = [];
        for (let Index = 0; Index < Input.targets.length; Index++) {
            process.stdout.write(
                `installer step=construct channel=${Input.channel} target=${Input.targets[Index].id} ` +
                `item=${Index + 2}/4\n`,
            );
            const Built = Buildˉtarget(Input, Input.targets[Index]);
            fs.writeFileSync(
                path.join(OutputRoot, Built.target.archive),
                Built.archiveBytes,
                { flag: "wx", mode: 0o644 },
            );
            BuiltTargets.push(Built);
        }
        process.stdout.write("installer step=report item=4/4\n");
        for (const Built of BuiltTargets) Printˉidentity(Built);
        process.stdout.write(`installer build status=Complete channel=${Input.channel} artifacts=2\n`);
    } else if (Command === "verify" && (Arguments.length === 1 || Arguments.length === 2)) {
        const Input = Loadˉinput(Arguments[1]);
        const Built = Verifyˉartifact(Input, Arguments[0]);
        Printˉidentity(Built);
        process.stdout.write(`installer verify status=Valid channel=${Input.channel} target=${Built.target.id}\n`);
    } else if (Command === "extract" && (Arguments.length === 2 || Arguments.length === 3)) {
        const Input = Loadˉinput(Arguments[2]);
        const Built = Verifyˉartifact(Input, Arguments[0]);
        Extractˉverified(Built, Arguments[1]);
        process.stdout.write(`installer extract status=Complete channel=${Input.channel} target=${Built.target.id}\n`);
    } else {
        process.stderr.write(
            "Usage: node Build-Installers.mjs " +
            "<build output-directory [input]|verify artifact [input]|" +
            "extract artifact output-directory [input]>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
