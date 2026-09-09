import { lstat, realpath } from 'node:fs/promises';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    Acquireˉcurrentˉsplitˉcompiler,
    Getˉcurrentˉsplitˉcompilerˉfamily,
    Getˉcurrentˉsplitˉcompilerˉkey,
} from './Current-Split-Compiler-Cache-Core.mjs';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';

const REPOSITORY = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const SUFFIX = process.platform === 'win32' ? '.exe' : '.elf';

try {
    if (process.argv.length !== 3) throw new Error('Usage: Build-Current-Publisher-Object-Tools.mjs <private-output-directory>');
    const Work = resolve(process.argv[2]);
    const Information = await lstat(Work);
    const Canonical = await realpath(Work);
    const Same = process.platform === 'win32'
        ? Canonical.toLowerCase() === Work.toLowerCase() : Canonical === Work;
    if (!Information.isDirectory() || Information.isSymbolicLink() ||
        !Same) {
        throw new Error('Publisher object tools require an ordinary canonical output directory.');
    }
    const Deadline = Date.now() + 240_000;
    const Key = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Compiler = await Acquireˉcurrentˉsplitˉcompiler(
        await Getˉcurrentˉsplitˉcompilerˉfamily(), Key,
        () => { throw new Error('Current compiler cache is absent; construct it in a separately approved run before this focused check.'); },
        async () => {
            if (await Getˉcurrentˉsplitˉcompilerˉkey() !== Key) {
                throw new Error('Current compiler inputs changed.');
            }
        },
    );
    const Products = [
        ['Current-Objects', 'Projects/Tests/Windvale-Native-Test-Current-Publisher-Object.wvproj'],
        ['Current-Structure', 'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Structure-Request-Tool.wvproj'],
    ];
    for (const [Index, [Name, Project]] of Products.entries()) {
        const Wvb = join(Work, Name + '.wvb');
        const Application = join(Work, Name + SUFFIX);
        for (const Output of [Wvb, Application]) {
            const Existing = await lstat(Output).catch(Error => {
                if (Error.code === 'ENOENT') return null;
                throw Error;
            });
            if (Existing !== null) throw new Error('Publisher object tool output already exists.');
        }
        process.stdout.write(`publisher object tools step=build item=${Index + 1}/2\n`);
        for (const Arguments of [
            [join(REPOSITORY, 'Tools/Native/Build-Cached-Split-Project-Wvb.mjs'),
                join(REPOSITORY, Project), Wvb,
                join(Compiler.directory, 'Analyzer' + SUFFIX),
                join(Compiler.directory, 'Analyzer.identity'),
                join(Compiler.directory, 'Emitter' + SUFFIX),
                join(Compiler.directory, 'Emitter.identity'), '--symbol-checkpoint'],
            [join(REPOSITORY, 'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs'),
                '7', Wvb, Application],
        ]) {
            const Result = await Runˉdevelopmentˉcommand(process.execPath, Arguments, Deadline, true);
            if (Result.Code !== 0 || Result.Error !== '') {
                throw new Error(`Publisher object tool construction failed (${Result.Code}): ${Result.Error}`);
            }
        }
    }
} catch (Error) {
    process.stderr.write(Error.message + '\n');
    process.exitCode = Error.exitCode ?? 1;
}
