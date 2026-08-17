import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";

function Fail(Message) {
    throw new Error(Message);
}

function Sha256(Value) {
    return crypto.createHash("sha256").update(Value).digest("hex");
}

function Run(Tool, Arguments, ExpectSuccess = true) {
    const Result = spawnSync(process.execPath, [Tool, ...Arguments], {
        cwd: RepositoryRoot,
        encoding: "utf8",
        windowsHide: true,
    });
    const Succeeded = Result.status === 0;
    if (Succeeded !== ExpectSuccess) {
        Fail(
            `Unexpected tool status=${Result.status}: ${Tool} ${Arguments.join(" ")}\n` +
            `${Result.stdout}${Result.stderr}`,
        );
    }
    return Result.stdout;
}

function Inventory(Root) {
    const Result = [];
    function Visit(Current, Prefix) {
        for (const Entry of fs.readdirSync(Current, { withFileTypes: true })) {
            const Relative = Prefix ? `${Prefix}/${Entry.name}` : Entry.name;
            const FullPath = path.join(Current, Entry.name);
            const Stat = fs.lstatSync(FullPath);
            if (Stat.isSymbolicLink()) Fail(`Test repository contains a link: ${Relative}`);
            if (Stat.isDirectory()) Visit(FullPath, Relative);
            else if (Stat.isFile()) {
                const Bytes = fs.readFileSync(FullPath);
                Result.push(`${Relative}|${Bytes.length}|${Sha256(Bytes)}`);
            } else Fail(`Test repository contains a special entry: ${Relative}`);
        }
    }
    Visit(Root, "");
    return Result.sort();
}

function Assertˉselection(Text, Expected) {
    const Lines = Text.trimEnd().split("\n");
    const Required = [
        "windvale-installer-selection 1",
        "version 0.2.0-dev.1",
        `target ${Expected.target}`,
        `profile ${Expected.profile}`,
        `object-count ${Expected.objects}`,
        `blob-count ${Expected.objects}`,
        `download-bytes ${Expected.downloadBytes}`,
        `expanded-bytes ${Expected.expandedBytes}`,
    ];
    if (Lines.length !== 8 + Expected.objects ||
        Required.some((Line, Index) => Lines[Index] !== Line)) {
        Fail(`Selection header differs for ${Expected.target}/${Expected.profile}.`);
    }
    const Objects = Lines.slice(8);
    for (const Component of Expected.components) {
        if (!Objects.some(Line => Line.startsWith(`object ${Component} `))) {
            Fail(`Selection omits component ${Component}.`);
        }
    }
    for (const Rejected of Expected.reject) {
        if (Objects.some(Line => Line.includes(Rejected))) {
            Fail(`Selection contains rejected path text: ${Rejected}`);
        }
    }
}

const RepositoryRoot = path.resolve(process.argv[2] ?? ".");
const Builder = path.join(RepositoryRoot, "Tools", "Release", "Build-Installer-Repository.mjs");
const Verifier = path.join(RepositoryRoot, "Tools", "Release", "Verify-Installer-Repository.mjs");
const TemporaryRoot = fs.realpathSync(os.tmpdir());
const Work = fs.mkdtempSync(path.join(TemporaryRoot, "windvale-installer-repository-"));

try {
    const First = path.join(Work, "First");
    const Second = path.join(Work, "Second");
    fs.mkdirSync(First);
    fs.mkdirSync(Second);

    process.stdout.write("native installer repository step=build-first item=1/12\n");
    Run(Builder, ["build", First]);

    process.stdout.write("native installer repository step=build-second item=2/12\n");
    Run(Builder, ["build", Second]);

    process.stdout.write("native installer repository step=prove-determinism item=3/12\n");
    const FirstInventory = Inventory(First);
    const SecondInventory = Inventory(Second);
    if (FirstInventory.join("\n") !== SecondInventory.join("\n") ||
        FirstInventory.length !== 16) {
        Fail("Installer repository trees are not byte-identical.");
    }

    process.stdout.write("native installer repository step=verify-complete item=4/12\n");
    const Report = Run(Verifier, ["verify", First]);
    if (!Report.includes(
        "status=Valid version=0.2.0-dev.1 targets=2 profiles=4 objects=15 " +
        "blobs=15 blob-bytes=9290710 index-sha256=" +
        "33fb6b436f28981489272478cb399ca7c857146950d6a5ac4adc4f062d6a0394")) {
        Fail("Installer repository verification report differs.");
    }

    const Selections = [
        {
            target: "windows-x64", profile: "runtime", objects: 3,
            downloadBytes: 550475, expandedBytes: 2517953,
            components: ["base", "runner", "verifier"], reject: ["linux-x64", "wvbuild"],
        },
        {
            target: "linux-x64", profile: "developer", objects: 6,
            downloadBytes: 4197615, expandedBytes: 36643777,
            components: ["assembler", "base", "compiler", "linker", "runner", "verifier"],
            reject: [".exe", "wvdump", "wvpublish"],
        },
        {
            target: "windows-x64", profile: "publisher", objects: 4,
            downloadBytes: 742573, expandedBytes: 3435457,
            components: ["base", "inspector", "publisher", "verifier"],
            reject: ["linux-x64", "wvbuild", "wvasm", "wvlink", "wvrun"],
        },
        {
            target: "linux-x64", profile: "full", objects: 8,
            downloadBytes: 4644930, expandedBytes: 38807478,
            components: [
                "assembler", "base", "compiler", "inspector",
                "linker", "publisher", "runner", "verifier",
            ],
            reject: [".exe", "windows-x64"],
        },
    ];
    for (let Index = 0; Index < Selections.length; Index++) {
        const Expected = Selections[Index];
        process.stdout.write(
            `native installer repository step=select-${Expected.profile} item=${Index + 5}/12\n`,
        );
        Assertˉselection(
            Run(Verifier, ["select", First, Expected.target, Expected.profile]),
            Expected,
        );
    }

    process.stdout.write("native installer repository step=reject-selection item=9/12\n");
    Run(Verifier, ["select", First, "macos-x64", "runtime"], false);
    Run(Verifier, ["select", First, "windows-x64", "unknown"], false);

    process.stdout.write("native installer repository step=reject-index-tamper item=10/12\n");
    const IndexPath = path.join(First, "Repository-Index.txt");
    const IndexBytes = fs.readFileSync(IndexPath);
    fs.appendFileSync(IndexPath, "x\n");
    Run(Verifier, ["verify", First], false);
    fs.writeFileSync(IndexPath, IndexBytes);

    process.stdout.write("native installer repository step=reject-object-state item=11/12\n");
    const ObjectRoot = path.join(First, "Objects", "sha256");
    const ObjectName = fs.readdirSync(ObjectRoot).sort()[0];
    const ObjectPath = path.join(ObjectRoot, ObjectName);
    const ObjectBytes = fs.readFileSync(ObjectPath);
    fs.appendFileSync(ObjectPath, Buffer.from([0]));
    Run(Verifier, ["verify", First], false);
    fs.writeFileSync(ObjectPath, ObjectBytes);
    const MissingPath = `${ObjectPath}.missing`;
    fs.renameSync(ObjectPath, MissingPath);
    Run(Verifier, ["verify", First], false);
    fs.renameSync(MissingPath, ObjectPath);
    const ExtraPath = path.join(ObjectRoot, "extra");
    fs.writeFileSync(ExtraPath, "extra\n");
    Run(Verifier, ["verify", First], false);
    fs.unlinkSync(ExtraPath);

    process.stdout.write("native installer repository step=reject-nonempty-output item=12/12\n");
    const Nonempty = path.join(Work, "Nonempty");
    fs.mkdirSync(Nonempty);
    fs.writeFileSync(path.join(Nonempty, "sentinel"), "preserve\n");
    Run(Builder, ["build", Nonempty], false);
    if (fs.readFileSync(path.join(Nonempty, "sentinel"), "utf8") !== "preserve\n") {
        Fail("Rejected repository output changed its sentinel.");
    }

    process.stdout.write(
        "native installer repository status=Passed cases=12 profiles=4 targets=2 " +
        "objects=15 blobs=15 deterministic=Verified selection=Verified tamper=Rejected\n",
    );
} finally {
    const Resolved = fs.realpathSync(Work);
    if (path.dirname(Resolved) !== TemporaryRoot ||
        !path.basename(Resolved).startsWith("windvale-installer-repository-")) {
        Fail(`Refusing to remove unexpected test path: ${Resolved}`);
    }
    fs.rmSync(Resolved, { recursive: true, force: true });
}
