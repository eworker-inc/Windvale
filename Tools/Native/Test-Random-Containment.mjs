import { mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { Testˉwvb, Testˉwvo } from "./Random-Containment-Binary.mjs";
import { Loadˉrandomˉcontainmentˉcorpus } from "./Random-Containment-Corpus.mjs";
import { Require } from "./Random-Containment-Host.mjs";
import { Testˉsource } from "./Random-Containment-Source.mjs";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Family = process.argv[2];
if (process.argv.length !== 3 || !["source", "wvb", "wvo"].includes(Family)) {
    console.error("Usage: node Tools/Native/Test-Random-Containment.mjs <source|wvb|wvo>");
    process.exit(64);
}
if (process.platform !== "win32" && process.platform !== "linux") {
    throw new Error(`The '${process.platform}' host is outside the containment profile.`);
}
if (process.arch !== "x64") {
    throw new Error(`The '${process.arch}' architecture is outside the containment profile.`);
}
if (typeof global.gc !== "function") {
    throw new Error("The containment runner requires Node.js --expose-gc.");
}

const Temporaryˉroot = path.resolve(tmpdir());
const Temporaryˉprefix = path.join(
    Temporaryˉroot,
    `windvale-random-containment-${Family}.`,
);
const Temporaryˉdirectory = await mkdtemp(Temporaryˉprefix);
try {
    const Corpus = await Loadˉrandomˉcontainmentˉcorpus(
        Repositoryˉroot,
        Temporaryˉdirectory,
    );
    const Cases = Corpus.filter(Item => Item.Family === Family);
    if (Family === "source") {
        await Testˉsource(Repositoryˉroot, Temporaryˉdirectory, Cases);
    } else if (Family === "wvb") {
        await Testˉwvb(Repositoryˉroot, Cases);
    } else {
        await Testˉwvo(Repositoryˉroot, Cases);
    }
    console.log(`Tests: ${Cases.length}, Passed: ${Cases.length}, Failed: 0`);
} finally {
    Require(
        path.dirname(Temporaryˉdirectory) === Temporaryˉroot &&
            path.basename(Temporaryˉdirectory).startsWith(
                `windvale-random-containment-${Family}.`,
            ),
        "Refusing to remove an unexpected containment directory.",
    );
    await rm(Temporaryˉdirectory, {
        recursive: true,
        force: true,
        maxRetries: 5,
        retryDelay: 100,
    });
}
