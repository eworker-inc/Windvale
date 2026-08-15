import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, "../..");
const GIT_ID = /^[0-9a-f]{40}$/;

const FILES = [
    ["approval", "windvale.wvb-inspector", "Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvapproval", "Policy/Windvale-Wvb-Inspector.wvapproval"],
    ["approval", "windvale.wvdb-query", "Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvapproval", "Policy/Windvale-Wvdb-Query.wvapproval"],
    ["launch-linux-x64", "windvale.wvb-inspector", "Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.linux-x64.wvlaunch", "Policy/Windvale-Wvb-Inspector.linux-x64.wvlaunch"],
    ["launch-linux-x64", "windvale.wvdb-query", "Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.linux-x64.wvlaunch", "Policy/Windvale-Wvdb-Query.linux-x64.wvlaunch"],
    ["launch-windows-x64", "windvale.wvb-inspector", "Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.windows-x64.wvlaunch", "Policy/Windvale-Wvb-Inspector.windows-x64.wvlaunch"],
    ["launch-windows-x64", "windvale.wvdb-query", "Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.windows-x64.wvlaunch", "Policy/Windvale-Wvdb-Query.windows-x64.wvlaunch"],
    ["provenance", "windvale.wvb-inspector", "Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvprov", "Policy/Windvale-Wvb-Inspector.wvprov"],
    ["provenance", "windvale.wvdb-query", "Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvprov", "Policy/Windvale-Wvdb-Query.wvprov"],
    ["verifier", "all", "Tools/Release/Verify-Release-Envelope.mjs", "Verify-Release-Envelope.mjs"],
];

function Fail(Message) {
    throw new Error(Message);
}

function Sha256(Bytes) {
    return crypto.createHash("sha256").update(Bytes).digest("hex");
}

function Emptyˉdirectory(DirectoryPath) {
    const Stat = fs.lstatSync(DirectoryPath);
    if (!Stat.isDirectory() || Stat.isSymbolicLink() || fs.readdirSync(DirectoryPath).length) {
        Fail("Stage-input output must be an existing empty ordinary directory.");
    }
    return fs.realpathSync(DirectoryPath);
}

function Readˉordinary(FilePath, Description) {
    const Stat = fs.lstatSync(FilePath);
    if (!Stat.isFile() || Stat.isSymbolicLink() || Stat.size < 1 ||
        Stat.size > 268_435_456) {
        Fail(`${Description} must be one bounded ordinary file.`);
    }
    const Bytes = fs.readFileSync(FilePath);
    if (Bytes.length !== Stat.size) Fail(`${Description} changed while it was read.`);
    return Bytes;
}

function Addˉartifact(Artifacts, Sources, Role, Target, RelativePath, Bytes) {
    const SourcePath = path.join(Sources, ...RelativePath.split("/"));
    fs.mkdirSync(path.dirname(SourcePath), { recursive: true });
    fs.writeFileSync(SourcePath, Bytes, { flag: "wx", mode: 0o644 });
    Artifacts.push({
        role: Role,
        target: Target,
        source: RelativePath,
        path: `Artifacts/${RelativePath}`,
        bytes: Bytes.length,
        sha256: Sha256(Bytes),
    });
}

function Createˉinput(WvdbBundlePath, InspectorBundlePath, Revision, Tree, OutputPath) {
    if (!GIT_ID.test(Revision) || !GIT_ID.test(Tree)) Fail("Stage Git identity differs.");
    const Output = Emptyˉdirectory(OutputPath);
    const Sources = path.join(Output, "Sources");
    fs.mkdirSync(Sources);
    const Artifacts = [];

    const WvdbBundle = Readˉordinary(WvdbBundlePath, "WVDB Query bundle");
    const InspectorBundle = Readˉordinary(InspectorBundlePath, "WVB Inspector bundle");
    if (WvdbBundle.length !== 43_725 ||
        Sha256(WvdbBundle) !== "3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474" ||
        InspectorBundle.length !== 92_781 ||
        Sha256(InspectorBundle) !== "a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9") {
        Fail("An exact package bundle identity differs.");
    }
    Addˉartifact(
        Artifacts,
        Sources,
        "package",
        "windvale.wvb-inspector",
        "Packages/Windvale-Wvb-Inspector.wvbundle",
        InspectorBundle,
    );
    Addˉartifact(
        Artifacts,
        Sources,
        "package",
        "windvale.wvdb-query",
        "Packages/Windvale-Wvdb-Query.wvbundle",
        WvdbBundle,
    );

    const LicenseText = Readˉordinary(path.join(REPOSITORY_ROOT, "LICENSE.md"), "License")
        .toString("utf8").replaceAll("\r\n", "\n");
    if (LicenseText.includes("\r") || LicenseText.includes("\0")) {
        Fail("The canonical license text differs.");
    }
    const License = Buffer.from(LicenseText, "utf8");
    if (License.length !== 13_249 ||
        Sha256(License) !== "26fc8ccf707d50fcd569353b594345ac234d4bf6e367b2b03cefe6027e108bef") {
        Fail("The canonical license identity differs.");
    }
    Addˉartifact(Artifacts, Sources, "license", "all", "LICENSE.md", License);

    for (const [Role, Target, Source, Destination] of FILES) {
        Addˉartifact(
            Artifacts,
            Sources,
            Role,
            Target,
            Destination,
            Readˉordinary(
                path.join(REPOSITORY_ROOT, ...Source.split("/")),
                `${Role} ${Target}`,
            ),
        );
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
        channel: "stage",
        sequence: 1,
        revision: Revision,
        tree: Tree,
        artifacts: Artifacts,
    };
    fs.writeFileSync(
        path.join(Output, "Root-Input.json"),
        `${JSON.stringify(RootInput, null, 2)}\n`,
        { flag: "wx", encoding: "utf8", mode: 0o644 },
    );
    fs.writeFileSync(
        path.join(Output, "Release-Input.json"),
        `${JSON.stringify(ReleaseInput, null, 2)}\n`,
        { flag: "wx", encoding: "utf8", mode: 0o644 },
    );
    process.stdout.write(
        `offline stage input status=Created packages=2 policy-records=8 artifacts=${Artifacts.length}\n`,
    );
}

const Arguments = process.argv.slice(2);
try {
    if (Arguments.length === 5) {
        Createˉinput(...Arguments);
    } else {
        process.stderr.write(
            "Usage: node Create-Offline-Package-Stage-Input.mjs " +
            "<wvdb-bundle> <inspector-bundle> <revision> <tree> <empty-output>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
