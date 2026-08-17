import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, "../..");
const PACKAGE_DIRECTORY = path.join(
    REPOSITORY_ROOT,
    "Distribution",
    "Applications",
    "Wvdb-Query",
);
const APPROVAL_NAME = "Windvale-Wvdb-Query.wvapproval";
const WINDOWS_NAME = "Windvale-Wvdb-Query.windows-x64.wvlaunch";
const LINUX_NAME = "Windvale-Wvdb-Query.linux-x64.wvlaunch";
const CAPABILITIES = [
    "console.write_line",
    "diagnostic.write_line",
    "filesystem.directory_read_v1",
    "process.argument",
    "process.argument_count",
];
const PACKAGE_IDENTITIES = {
    manifest: [
        "Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack",
        866,
        "835f573302377fdd38e4c3d51fa9106397beba0b9813f99bfc3143d08a156406",
    ],
    lock: [
        "Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock",
        1770,
        "7fe9552317e0845b693b8a4ade1882c4a492cecf46c1bfcaaf26b45ed067be50",
    ],
    provenance: [
        "Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvprov",
        452,
        "c3bc4d43371947f9005c9af6975207119f22e5bc0b709ea8cd8c8f6ea64090b9",
    ],
};
const APPROVAL_BYTES = 927;
const APPROVAL_SHA256 = "0b58e435e08045a7118353e2a454e92f4eadfdf36458693e5dd1cf85b58dfdb2";
const WINDOWS_BYTES = 1_315;
const WINDOWS_SHA256 = "9012947e64e3650bba1e5dd5213a1f4c78c56318e5b5ff40804d7ca902aa3348";
const LINUX_BYTES = 1_310;
const LINUX_SHA256 = "1e61ebeac166c9b35cb852dc290ae5df97cf289b22fa8e057b4df74953786dfc";
const BUNDLE_SHA256 = "40c09378e20b5ac49d41fada61c24e786363e89bf839925cac8d9f3c715a9378";
const WVB_SHA256 = "77cb6034402942734be316b9a135d6c1b46ace5cb43a198b2aafe2d1b098027b";
const INSPECTOR_DIRECTORY = path.join(
    REPOSITORY_ROOT,
    "Distribution",
    "Applications",
    "Wvb-Inspector",
);
const INSPECTOR_APPROVAL_NAME = "Windvale-Wvb-Inspector.wvapproval";
const INSPECTOR_WINDOWS_NAME = "Windvale-Wvb-Inspector.windows-x64.wvlaunch";
const INSPECTOR_LINUX_NAME = "Windvale-Wvb-Inspector.linux-x64.wvlaunch";
const INSPECTOR_CAPABILITIES = [
    "console.write_line",
    "diagnostic.write_line",
    "file.read_bytes",
    "process.argument",
    "process.argument_count",
];
const INSPECTOR_IDENTITIES = {
    manifest: [
        "Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack",
        412,
        "a58441a48b0e11c4062e77b0176934952c1de238c78d04ba88ca9ca61e0a41b6",
    ],
    lock: [
        "Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvlock",
        1021,
        "eef8bd6d8ab5c535d263fb914fa3fae6f82ee9ae16b0854de497749475f76ad1",
    ],
    provenance: [
        "Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvprov",
        455,
        "e3d9ddf823a0fd4fab6406de97f3733cd1f5a802daf20dc15cf1d6d7de8ce44f",
    ],
};
const INSPECTOR_APPROVAL_BYTES = 923;
const INSPECTOR_APPROVAL_SHA256 = "32023a688e3ab4eb6dd83f72c349bf7d2b7ddb184b49253819075f8d9af7b69f";
const INSPECTOR_BUNDLE_SHA256 = "a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9";
const INSPECTOR_WVB_SHA256 = "293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753";
const INSPECTOR_WINDOWS_BYTES = 1_000;
const INSPECTOR_WINDOWS_SHA256 = "eac1706bc237f60b0a843cb369f5b3f07cff794d44d07079c557e1f04f9fa47b";
const INSPECTOR_LINUX_BYTES = 996;
const INSPECTOR_LINUX_SHA256 = "f5c45df84c9624fd7579fc83947a595caf206ddb5783a9b3efba15d7ad6e379b";

function Fail(Message) {
    throw new Error(Message);
}

function Sha256(Bytes) {
    return crypto.createHash("sha256").update(Bytes).digest("hex");
}

function Readˉcanonical(FilePath, Description) {
    const Stat = fs.lstatSync(FilePath);
    if (!Stat.isFile() || Stat.isSymbolicLink()) Fail(`${Description} is not an ordinary file.`);
    if (!Number.isSafeInteger(Stat.size) || Stat.size < 1 || Stat.size > 4_096) {
        Fail(`${Description} exceeds its byte limit.`);
    }
    const Bytes = fs.readFileSync(FilePath);
    if (Bytes.length !== Stat.size) Fail(`${Description} changed while it was read.`);
    const Text = Bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Bytes) || Text.includes("\r") ||
        Text.includes("\0") || !Text.endsWith("\n")) {
        Fail(`${Description} is not canonical LF UTF-8.`);
    }
    return { bytes: Bytes, lines: Text.slice(0, -1).split("\n") };
}

function Verifyˉbinaryˉidentity(RelativePath, ExpectedBytes, ExpectedSha256, Description) {
    const FilePath = path.join(REPOSITORY_ROOT, ...RelativePath.split("/"));
    const Stat = fs.lstatSync(FilePath);
    if (!Stat.isFile() || Stat.isSymbolicLink() || Stat.size !== ExpectedBytes) {
        Fail(`The ${Description} identity differs.`);
    }
    const Bytes = fs.readFileSync(FilePath);
    if (Bytes.length !== ExpectedBytes || Sha256(Bytes) !== ExpectedSha256) {
        Fail(`The ${Description} identity differs.`);
    }
}

function Assertˉlines(Observed, Expected, Description) {
    if (Observed.length !== Expected.length) Fail(`${Description} line count differs.`);
    for (let Index = 0; Index < Expected.length; Index++) {
        if (Observed[Index] !== Expected[Index]) {
            Fail(`${Description} differs at record ${Index + 1}.`);
        }
    }
}

function Verifyˉpackageˉidentity(Identities, Key, Description) {
    const [RelativePath, ExpectedBytes, ExpectedSha256] = Identities[Key];
    const Value = Readˉcanonical(path.join(REPOSITORY_ROOT, ...RelativePath.split("/")), Key);
    if (Value.bytes.length !== ExpectedBytes || Sha256(Value.bytes) !== ExpectedSha256) {
        Fail(`The ${Description} ${Key} identity differs.`);
    }
    return Value;
}

function Verifyˉclosure(Package, Lock, Capabilities, Description) {
    const Expected = Capabilities.map(Name => `capability ${Name}`);
    const PackageCapabilities = Package.lines.filter(Line => Line.startsWith("capability "));
    const LockCapabilities = Lock.lines.filter(Line => Line.startsWith("capability "));
    Assertˉlines(PackageCapabilities, Expected, `${Description} package capability closure`);
    Assertˉlines(LockCapabilities, Expected, `${Description} lock capability closure`);
}

function Expectedˉapproval() {
    return [
        "windvale-capability-approval 1",
        "application windvale.wvdb-query 0.1.0",
        "target hosted-wvb-v1",
        "package-manifest 835f573302377fdd38e4c3d51fa9106397beba0b9813f99bfc3143d08a156406 866",
        "lock 7fe9552317e0845b693b8a4ade1882c4a492cecf46c1bfcaaf26b45ed067be50 1770",
        `bundle ${BUNDLE_SHA256} 43598`,
        "provenance c3bc4d43371947f9005c9af6975207119f22e5bc0b709ea8cd8c8f6ea64090b9 452",
        `executable ${WVB_SHA256} 26145`,
        "capability-count 5",
        "approve 0 console.write_line standard-output-line-v1",
        "approve 1 diagnostic.write_line standard-diagnostic-line-v1",
        "approve 2 filesystem.directory_read_v1 fixed-read-only-object-v1",
        "approve 3 process.argument immutable-argument-snapshot-v1",
        "approve 4 process.argument_count immutable-argument-snapshot-v1",
        "deny ambient-filesystem",
        "deny file-mutation",
        "deny environment",
        "deny network",
        "deny process-launch",
        "deny clock",
        "deny entropy",
    ];
}

function Expectedˉinspectorˉapproval() {
    return [
        "windvale-capability-approval 1",
        "application windvale.wvb-inspector 0.1.0",
        "target hosted-wvb-v1",
        "package-manifest a58441a48b0e11c4062e77b0176934952c1de238c78d04ba88ca9ca61e0a41b6 412",
        "lock eef8bd6d8ab5c535d263fb914fa3fae6f82ee9ae16b0854de497749475f76ad1 1021",
        `bundle ${INSPECTOR_BUNDLE_SHA256} 92781`,
        "provenance e3d9ddf823a0fd4fab6406de97f3733cd1f5a802daf20dc15cf1d6d7de8ce44f 455",
        `executable ${INSPECTOR_WVB_SHA256} 76527`,
        "capability-count 5",
        "approve 0 console.write_line standard-output-line-v1",
        "approve 1 diagnostic.write_line standard-diagnostic-line-v1",
        "approve 2 file.read_bytes explicit-host-file-read-only-v1",
        "approve 3 process.argument immutable-argument-snapshot-v1",
        "approve 4 process.argument_count immutable-argument-snapshot-v1",
        "deny ambient-filesystem",
        "deny file-mutation",
        "deny environment",
        "deny network",
        "deny process-launch",
        "deny clock",
        "deny entropy",
    ];
}

function Expectedˉinspectorˉlaunch(Target) {
    const Windows = Target === "windows-x64";
    return [
        "windvale-launch-record 2",
        "application windvale.wvb-inspector 0.1.0",
        `generation 0.1.0-${Target}-32023a688e3a`,
        `target ${Target}`,
        `approval ${INSPECTOR_APPROVAL_SHA256} ${INSPECTOR_APPROVAL_BYTES}`,
        `bundle ${INSPECTOR_BUNDLE_SHA256} 92781`,
        "lock eef8bd6d8ab5c535d263fb914fa3fae6f82ee9ae16b0854de497749475f76ad1 1021",
        `wvb ${INSPECTOR_WVB_SHA256} 76527`,
        Windows ?
            "host-application 61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381 795136" :
            "host-application d3215e8345bf5cd9f3265b8421cf57d456ae605c5493fcc215a3e11daab44627 794624",
        "entry Main",
        "provider-table 2 5",
        "bind 0 console.write_line host-standard-output line-lf",
        "bind 1 diagnostic.write_line host-standard-diagnostic line-lf",
        "bind 2 file.read_bytes host-read-only-file argument-0",
        "bind 3 process.argument immutable-launch-arguments 1",
        "bind 4 process.argument_count immutable-launch-arguments 1",
        "argument-count 1",
        "argument 0 host-path-utf8 1 4096",
        "deny path-enumeration",
        "deny file-mutation",
        "deny environment",
        "deny network",
        "deny process-launch",
        "deny clock",
        "deny entropy",
    ];
}

function Expectedˉlaunch(Target) {
    const Windows = Target === "windows-x64";
    return [
        "windvale-launch-record 1",
        "application windvale.wvdb-query 0.1.0",
        `generation 0.1.0-${Target}-0b58e435e080`,
        `target ${Target}`,
        `approval ${APPROVAL_SHA256} ${APPROVAL_BYTES}`,
        `bundle ${BUNDLE_SHA256} 43598`,
        `wvb ${WVB_SHA256} 26145`,
        "directory-host-wvo 7ab58a817fe5dbc8e8f91b910654487ba62e10bc5aa5d1ae74b6bb07f2f6ca09 2010",
        Windows ?
            "platform-leaf-wvo d2da1c67864c242aeb9797661028295922486de2cf7d37aa41024189afb10f34 1951" :
            "platform-leaf-wvo 0ccbcda71b20eaa024946e4fbb2016853952a39f1fe58ed0a183bde502335d86 681",
        Windows ?
            "linked-image 0abfe3194e1d412bf78b812484ac157e858efad576573e64f901e323ae20175d 236856" :
            "linked-image c7b1971f792f90ed94b32abbbb4355b8ac84773ec8599127b5a37cbb40bec872 235837",
        Windows ?
            "host-application 5780f7416938fa1329c6e85314697c81a8a29fcd35f792b7c7b5353962e944d7 256512" :
            "host-application c457ac0470385fedc4e328abe29a4c56d2253abb3c5d91ffe2c8ead24257401c 258048",
        "abi 23",
        "entry Directory_host_entry 233760",
        "provider-table 1 5",
        "bind 0 console.write_line host-standard-output line-lf",
        "bind 1 diagnostic.write_line host-standard-diagnostic line-lf",
        "bind 2 filesystem.directory_read_v1 fixed-read-only-object Windvale-Database-Storage.bin 3072",
        "bind 3 process.argument immutable-launch-arguments 2",
        "bind 4 process.argument_count immutable-launch-arguments 2",
        "argument 0 exact-utf8 Windvale-Database-Storage.bin",
        "argument 1 unsigned-decimal-u64 1 20",
        "deny native-path",
        "deny file-mutation",
        "deny directory-enumeration",
        "deny environment",
        "deny network",
        "deny process-launch",
        "deny clock",
        "deny entropy",
    ];
}

function Verifyˉrecords(RecordsPath) {
    const RecordsStat = fs.lstatSync(RecordsPath);
    if (!RecordsStat.isDirectory() || RecordsStat.isSymbolicLink()) {
        Fail("The approval record root is not an ordinary directory.");
    }
    const RecordsRoot = fs.realpathSync(RecordsPath);
    process.stdout.write("wvdb approval step=verify-package item=1/4\n");
    const Package = Verifyˉpackageˉidentity(PACKAGE_IDENTITIES, "manifest", "WVDB Query");
    const Lock = Verifyˉpackageˉidentity(PACKAGE_IDENTITIES, "lock", "WVDB Query");
    Verifyˉpackageˉidentity(PACKAGE_IDENTITIES, "provenance", "WVDB Query");
    Verifyˉclosure(Package, Lock, CAPABILITIES, "WVDB Query");

    process.stdout.write("wvdb approval step=verify-approval item=2/4\n");
    const Approval = Readˉcanonical(path.join(RecordsRoot, APPROVAL_NAME), "Approval record");
    if (Approval.bytes.length !== APPROVAL_BYTES || Sha256(Approval.bytes) !== APPROVAL_SHA256) {
        Fail("The approval identity differs.");
    }
    Assertˉlines(Approval.lines, Expectedˉapproval(), "Approval record");

    process.stdout.write("wvdb approval step=verify-launch-records item=3/4 targets=2\n");
    const Windows = Readˉcanonical(path.join(RecordsRoot, WINDOWS_NAME), "Windows launch record");
    const Linux = Readˉcanonical(path.join(RecordsRoot, LINUX_NAME), "Linux launch record");
    if (Windows.bytes.length !== WINDOWS_BYTES || Sha256(Windows.bytes) !== WINDOWS_SHA256 ||
        Linux.bytes.length !== LINUX_BYTES || Sha256(Linux.bytes) !== LINUX_SHA256) {
        Fail("A target launch-record identity differs.");
    }
    Assertˉlines(Windows.lines, Expectedˉlaunch("windows-x64"), "Windows launch record");
    Assertˉlines(Linux.lines, Expectedˉlaunch("linux-x64"), "Linux launch record");
    if (Sha256(Windows.bytes) === Sha256(Linux.bytes)) {
        Fail("Target launch identities must differ.");
    }

    process.stdout.write("wvdb approval step=report item=4/4\n");
    process.stdout.write(
        `wvdb approval status=Valid records=3 capabilities=5 targets=2 ` +
        `approval=${APPROVAL_SHA256} windows=${WINDOWS_SHA256} ` +
        `linux=${LINUX_SHA256}\n`,
    );
}

function Verifyˉinspectorˉrecords(RecordsPath) {
    const RecordsStat = fs.lstatSync(RecordsPath);
    if (!RecordsStat.isDirectory() || RecordsStat.isSymbolicLink()) {
        Fail("The inspector approval record root is not an ordinary directory.");
    }
    const RecordsRoot = fs.realpathSync(RecordsPath);
    process.stdout.write("inspector approval step=verify-package-and-hosts item=1/4 targets=2\n");
    const Package = Verifyˉpackageˉidentity(
        INSPECTOR_IDENTITIES,
        "manifest",
        "WVB Inspector",
    );
    const Lock = Verifyˉpackageˉidentity(INSPECTOR_IDENTITIES, "lock", "WVB Inspector");
    Verifyˉpackageˉidentity(INSPECTOR_IDENTITIES, "provenance", "WVB Inspector");
    Verifyˉclosure(Package, Lock, INSPECTOR_CAPABILITIES, "WVB Inspector");
    Verifyˉbinaryˉidentity(
        "Artifacts/Native-Front-Door/windows-x64/wvdump.exe",
        795_136,
        "61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381",
        "WVB Inspector Windows host application",
    );
    Verifyˉbinaryˉidentity(
        "Artifacts/Native-Front-Door/linux-x64/wvdump.elf",
        794_624,
        "d3215e8345bf5cd9f3265b8421cf57d456ae605c5493fcc215a3e11daab44627",
        "WVB Inspector Linux host application",
    );

    process.stdout.write("inspector approval step=verify-approval item=2/4\n");
    const Approval = Readˉcanonical(
        path.join(RecordsRoot, INSPECTOR_APPROVAL_NAME),
        "Inspector approval record",
    );
    if (Approval.bytes.length !== INSPECTOR_APPROVAL_BYTES ||
        Sha256(Approval.bytes) !== INSPECTOR_APPROVAL_SHA256) {
        Fail("The inspector approval identity differs.");
    }
    Assertˉlines(Approval.lines, Expectedˉinspectorˉapproval(), "Inspector approval record");

    process.stdout.write("inspector approval step=verify-launch-records item=3/4 targets=2\n");
    const Windows = Readˉcanonical(
        path.join(RecordsRoot, INSPECTOR_WINDOWS_NAME),
        "Inspector Windows launch record",
    );
    const Linux = Readˉcanonical(
        path.join(RecordsRoot, INSPECTOR_LINUX_NAME),
        "Inspector Linux launch record",
    );
    if (Windows.bytes.length !== INSPECTOR_WINDOWS_BYTES ||
        Sha256(Windows.bytes) !== INSPECTOR_WINDOWS_SHA256 ||
        Linux.bytes.length !== INSPECTOR_LINUX_BYTES ||
        Sha256(Linux.bytes) !== INSPECTOR_LINUX_SHA256) {
        Fail("An inspector target launch-record identity differs.");
    }
    Assertˉlines(
        Windows.lines,
        Expectedˉinspectorˉlaunch("windows-x64"),
        "Inspector Windows launch record",
    );
    Assertˉlines(
        Linux.lines,
        Expectedˉinspectorˉlaunch("linux-x64"),
        "Inspector Linux launch record",
    );

    process.stdout.write("inspector approval step=report item=4/4\n");
    process.stdout.write(
        `inspector approval status=Valid records=3 capabilities=5 targets=2 ` +
        `approval=${INSPECTOR_APPROVAL_SHA256} windows=${INSPECTOR_WINDOWS_SHA256} ` +
        `linux=${INSPECTOR_LINUX_SHA256}\n`,
    );
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "verify" && Arguments.length <= 1) {
        Verifyˉrecords(Arguments[0] ?? PACKAGE_DIRECTORY);
    } else if (Command === "verify-inspector" && Arguments.length <= 1) {
        Verifyˉinspectorˉrecords(Arguments[0] ?? INSPECTOR_DIRECTORY);
    } else {
        process.stderr.write(
            "Usage: node Verify-Wvdb-Approval-Records.mjs <verify|verify-inspector> " +
            "[records-directory]\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
