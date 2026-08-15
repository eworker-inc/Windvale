import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

const ENTRIES = [
    ["approval", "all", "approval.txt"],
    ["installer", "linux-x64", "windvale-linux.tar.gz"],
    ["installer", "windows-x64", "windvale-windows.zip"],
    ["license", "all", "LICENSE.md"],
    ["package", "windvale.wvb-inspector", "wvb-inspector.wvbundle"],
    ["package", "windvale.wvdb-query", "wvdb-query.wvbundle"],
    ["provenance", "all", "provenance.txt"],
    ["qualification", "linux-x64", "qualification-linux.txt"],
    ["qualification", "windows-x64", "qualification-windows.txt"],
    ["recovery", "stage0", "stage0-recovery.txt"],
    ["source", "all", "windvale-source.tar.gz"],
    ["verifier", "all", "Verify-Release-Envelope.mjs"],
];

function Fail(Message) {
    throw new Error(Message);
}

function Emptyˉdirectory(DirectoryPath, Description) {
    const Stat = fs.lstatSync(DirectoryPath);
    if (!Stat.isDirectory() || Stat.isSymbolicLink() || fs.readdirSync(DirectoryPath).length) {
        Fail(`${Description} must be an existing empty ordinary directory.`);
    }
    return fs.realpathSync(DirectoryPath);
}

function Sha256(Bytes) {
    return crypto.createHash("sha256").update(Bytes).digest("hex");
}

function Writeˉtext(FilePath, Text) {
    fs.mkdirSync(path.dirname(FilePath), { recursive: true });
    fs.writeFileSync(FilePath, Text, { flag: "wx", encoding: "utf8", mode: 0o644 });
}

function Createˉfixture(OutputPath) {
    const Output = Emptyˉdirectory(OutputPath, "Fixture output");
    const Source = path.join(Output, "Sources");
    fs.mkdirSync(Source);
    const Artifacts = [];
    for (const [Role, Target, Name] of ENTRIES) {
        const Bytes = Buffer.from(`windvale release fixture role=${Role} target=${Target}\n`, "utf8");
        const SourcePath = `${Role}-${Target}-${Name}`;
        fs.writeFileSync(path.join(Source, SourcePath), Bytes, { flag: "wx", mode: 0o644 });
        Artifacts.push({
            role: Role,
            target: Target,
            source: SourcePath,
            path: `Artifacts/${Name}`,
            bytes: Bytes.length,
            sha256: Sha256(Bytes),
        });
    }
    const RootInput = {
        format: "windvale-release-root-input-1",
        policyGeneration: 1,
        versionPrefix: "0.1.",
        minimumSequence: 1,
        maximumSequence: 9,
    };
    const ReleaseInput = {
        format: "windvale-release-envelope-input-1",
        version: "0.1.0",
        channel: "preview",
        sequence: 1,
        revision: "0123456789abcdef0123456789abcdef01234567",
        tree: "89abcdef0123456789abcdef0123456789abcdef",
        artifacts: Artifacts,
    };
    Writeˉtext(path.join(Output, "Root-Input.json"), `${JSON.stringify(RootInput, null, 2)}\n`);
    Writeˉtext(path.join(Output, "Release-Input.json"), `${JSON.stringify(ReleaseInput, null, 2)}\n`);
    process.stdout.write("release fixture status=Created artifacts=12 packages=2\n");
}

function Copyˉtree(SourcePath, OutputPath) {
    const SourceStat = fs.lstatSync(SourcePath);
    if (!SourceStat.isDirectory() || SourceStat.isSymbolicLink()) {
        Fail("Fixture source must be an ordinary directory.");
    }
    const Output = Emptyˉdirectory(OutputPath, "Fixture copy output");
    fs.cpSync(fs.realpathSync(SourcePath), Output, {
        recursive: true,
        errorOnExist: true,
        force: false,
        dereference: false,
    });
}

function Inventory(DirectoryPath) {
    const Root = fs.realpathSync(DirectoryPath);
    const Result = [];
    function Visit(Current, Prefix) {
        for (const Entry of fs.readdirSync(Current, { withFileTypes: true })) {
            const Relative = Prefix ? `${Prefix}/${Entry.name}` : Entry.name;
            const Candidate = path.join(Current, Entry.name);
            const Stat = fs.lstatSync(Candidate);
            if (Stat.isSymbolicLink()) Fail(`Fixture contains a link: ${Relative}`);
            if (Stat.isDirectory()) Visit(Candidate, Relative);
            else if (Stat.isFile()) Result.push([Relative, fs.readFileSync(Candidate)]);
            else Fail(`Fixture contains a special entry: ${Relative}`);
        }
    }
    Visit(Root, "");
    return Result.sort((Left, Right) => Buffer.from(Left[0]).compare(Buffer.from(Right[0])));
}

function Compareˉtrees(LeftPath, RightPath) {
    const Left = Inventory(LeftPath);
    const Right = Inventory(RightPath);
    if (Left.length !== Right.length) Fail("Fixture tree file count differs.");
    for (let Index = 0; Index < Left.length; Index++) {
        if (Left[Index][0] !== Right[Index][0] || !Left[Index][1].equals(Right[Index][1])) {
            Fail(`Fixture tree differs: ${Left[Index][0]}`);
        }
    }
    process.stdout.write(`release fixture compare status=Equal files=${Left.length}\n`);
}

function Mutateˉinput(Kind, InputPath, OutputPath) {
    const Input = JSON.parse(fs.readFileSync(InputPath, "utf8"));
    if (Kind === "unsafe-path") {
        Input.artifacts[0].path = "Artifacts/../escape.txt";
    } else if (Kind === "missing-profile") {
        Input.artifacts = Input.artifacts.filter(Artifact =>
            !(Artifact.role === "recovery" && Artifact.target === "stage0"));
    } else if (Kind === "mixed-package-profile") {
        const Package = Input.artifacts.find(Artifact => Artifact.role === "package");
        Input.artifacts.push({
            ...Package,
            target: "all",
            path: "Artifacts/legacy-package.wvbundle",
        });
    } else {
        Fail(`Unknown release fixture mutation: ${Kind}`);
    }
    Writeˉtext(OutputPath, `${JSON.stringify(Input, null, 2)}\n`);
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "create" && Arguments.length === 1) {
        Createˉfixture(Arguments[0]);
    } else if (Command === "copy" && Arguments.length === 2) {
        Copyˉtree(Arguments[0], Arguments[1]);
    } else if (Command === "compare" && Arguments.length === 2) {
        Compareˉtrees(Arguments[0], Arguments[1]);
    } else if (Command === "mutate-input" && Arguments.length === 3) {
        Mutateˉinput(Arguments[0], Arguments[1], Arguments[2]);
    } else {
        process.stderr.write(
            "Usage: node Create-Release-Envelope-Fixture.mjs " +
            "<create output|copy source output|compare left right|" +
            "mutate-input unsafe-path|missing-profile|mixed-package-profile input output>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
