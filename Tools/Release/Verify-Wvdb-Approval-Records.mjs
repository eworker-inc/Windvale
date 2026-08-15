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
        1750,
        "ad22e10e41dda772650123b4802518575088973aa73277889b443ad27aa25618",
    ],
    provenance: [
        "Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvprov",
        452,
        "0030feba6327489c13de0ef019705a0c018945e8669fed695d76465ff4d4a3e5",
    ],
};
const APPROVAL_BYTES = 927;
const APPROVAL_SHA256 = "3c4a968745cde9d5073c67c6c453443d54c74e779b509c2f00131b4d47e8ef71";
const WINDOWS_BYTES = 1_315;
const WINDOWS_SHA256 = "95d1a64007f487e57aec77f7466d091cc54247dcbec2f8534b5870e36715b0b3";
const LINUX_BYTES = 1_310;
const LINUX_SHA256 = "b0c976649936cf43cfa1ccb79a63093e584dda9b22cf905b954db6e3192eacd5";
const BUNDLE_SHA256 = "3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474";
const WVB_SHA256 = "61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2";

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

function Assertˉlines(Observed, Expected, Description) {
    if (Observed.length !== Expected.length) Fail(`${Description} line count differs.`);
    for (let Index = 0; Index < Expected.length; Index++) {
        if (Observed[Index] !== Expected[Index]) {
            Fail(`${Description} differs at record ${Index + 1}.`);
        }
    }
}

function Verifyˉpackageˉidentity(Key) {
    const [RelativePath, ExpectedBytes, ExpectedSha256] = PACKAGE_IDENTITIES[Key];
    const Value = Readˉcanonical(path.join(REPOSITORY_ROOT, ...RelativePath.split("/")), Key);
    if (Value.bytes.length !== ExpectedBytes || Sha256(Value.bytes) !== ExpectedSha256) {
        Fail(`The ${Key} identity differs.`);
    }
    return Value;
}

function Verifyˉclosure(Package, Lock) {
    const Expected = CAPABILITIES.map(Name => `capability ${Name}`);
    const PackageCapabilities = Package.lines.filter(Line => Line.startsWith("capability "));
    const LockCapabilities = Lock.lines.filter(Line => Line.startsWith("capability "));
    Assertˉlines(PackageCapabilities, Expected, "Package capability closure");
    Assertˉlines(LockCapabilities, Expected, "Lock capability closure");
}

function Expectedˉapproval() {
    return [
        "windvale-capability-approval 1",
        "application windvale.wvdb-query 0.1.0",
        "target hosted-wvb-v1",
        "package-manifest 835f573302377fdd38e4c3d51fa9106397beba0b9813f99bfc3143d08a156406 866",
        "lock ad22e10e41dda772650123b4802518575088973aa73277889b443ad27aa25618 1750",
        `bundle ${BUNDLE_SHA256} 43725`,
        "provenance 0030feba6327489c13de0ef019705a0c018945e8669fed695d76465ff4d4a3e5 452",
        `executable ${WVB_SHA256} 26294`,
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

function Expectedˉlaunch(Target) {
    const Windows = Target === "windows-x64";
    return [
        "windvale-launch-record 1",
        "application windvale.wvdb-query 0.1.0",
        `generation 0.1.0-${Target}-3c4a968745cd`,
        `target ${Target}`,
        `approval ${APPROVAL_SHA256} ${APPROVAL_BYTES}`,
        `bundle ${BUNDLE_SHA256} 43725`,
        `wvb ${WVB_SHA256} 26294`,
        "directory-host-wvo 7ab58a817fe5dbc8e8f91b910654487ba62e10bc5aa5d1ae74b6bb07f2f6ca09 2010",
        Windows ?
            "platform-leaf-wvo d1dc38e751ab7a04cb115f2fc6f0e62a5452e2937cc1dd56867f3da8fe2ddc03 1731" :
            "platform-leaf-wvo 53136d316adec7f6b7667ecc853764fc5207d25fc52e60d2175cd8e0f49c4c64 608",
        Windows ?
            "linked-image 60bdf794d8fba0889a077eeec35fab75de9fd174a5a894eb78ef316ad1c8872c 238413" :
            "linked-image 76b8327d6f970c467d76a4e9c2f64d7473897d2afe2a444c007f840e42a35632 237437",
        Windows ?
            "host-application 7cd60860e07294d9a45064495da33a42cc752849accfc672c35a69454cd963d8 258048" :
            "host-application 29b4d4db7505daec94865d423e3805b02bde95751343b1fb7e4ceee8045a202d 258048",
        "abi 23",
        "entry Directory_host_entry 235440",
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
    const Package = Verifyˉpackageˉidentity("manifest");
    const Lock = Verifyˉpackageˉidentity("lock");
    Verifyˉpackageˉidentity("provenance");
    Verifyˉclosure(Package, Lock);

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

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "verify" && Arguments.length <= 1) {
        Verifyˉrecords(Arguments[0] ?? PACKAGE_DIRECTORY);
    } else {
        process.stderr.write(
            "Usage: node Verify-Wvdb-Approval-Records.mjs verify [records-directory]\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
