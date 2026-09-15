import { createHash, randomBytes } from 'node:crypto';
import { lstat, mkdir, readFile, realpath } from 'node:fs/promises';
import path from 'node:path';
import { pathToFileURL } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { REPOSITORY_ROOT } from './Native-Project-Cache-Key-Core.mjs';

// This is source provenance, not a replacement executable trust root. The
// recorded tree reconstructs through its existing qualified WVB bootstrap pins.
export const SOURCE_EDITION_PREDECESSOR = '992ba7bbebbbf3356966bdcc0ea1d2b0e5923720';
const PREDECESSOR_TREE = 'c6231310843c24a03b0253f6f5a062d77db9f73f';
const MAXIMUM_SOURCE_BYTES = 805_306_368;
const MAXIMUM_SOURCE_FILES = 8_192;
const WINDOWS = process.platform === 'win32';
const SUFFIX = WINDOWS ? '.exe' : '.elf';

export async function Constructˉsourceˉeditionˉpredecessor(Work) {
    Work = path.resolve(Work);
    if (!Sameˉpath(await realpath(Work), Work) || !(await lstat(Work)).isDirectory()) {
        throw new Error('The predecessor work directory must be canonical.');
    }
    const Source = path.join(Work, 'Predecessor-' + randomBytes(12).toString('hex'));
    const Products = path.join(Work, 'Predecessor-Products');
    await mkdir(Products);
    let Registered = false;
    async function Run(Label, Command, Arguments, Maximum = 600_000, Stream = true) {
        console.log(`source-edition predecessor step=${Label} status=Started`);
        const Result = await Runˉdevelopmentˉcommand(Command, Arguments,
            Date.now() + Maximum, Stream, 1_048_576);
        if (Result.Code !== 0) {
            throw new Error(`Predecessor ${Label} failed: ${Result.Error || Result.Output}`);
        }
        console.log(`source-edition predecessor step=${Label} status=Complete`);
        return Result.Output;
    }
    const Git = (Label, Arguments) => Run(Label, 'git', ['-C', REPOSITORY_ROOT, ...Arguments],
        120_000, Label !== 'source-inventory');
    try {
        const Tree = await Git('source-identity', ['rev-parse', `${SOURCE_EDITION_PREDECESSOR}^{tree}`]);
        if (Tree.trim() !== PREDECESSOR_TREE) throw new Error('The predecessor Git source tree differs.');
        const Inventory = await Git('source-inventory', ['ls-tree', '-rlz', SOURCE_EDITION_PREDECESSOR]);
        const Entries = Inventory.split('\0').filter(Boolean);
        if (Entries.length === 0 || Entries.length > MAXIMUM_SOURCE_FILES) {
            throw new Error('The predecessor source inventory exceeds its file bound.');
        }
        let Bytes = 0;
        for (const Entry of Entries) {
            const Match = /^(100644|100755) blob [0-9a-f]{40}\s+(\d+)\t([^\r\n\0]+)$/u.exec(Entry);
            if (Match === null || Match[3].split('/').some(Part =>
                Part === '' || Part === '.' || Part === '..' || Part.includes('\\') || Part.includes(':'))) {
                throw new Error('The predecessor inventory contains an unsupported file or path.');
            }
            Bytes += Number(Match[2]);
            if (!Number.isSafeInteger(Bytes) || Bytes > MAXIMUM_SOURCE_BYTES) {
                throw new Error('The predecessor source inventory exceeds 768 MiB.');
            }
        }
        console.log(`source-edition predecessor files=${Entries.length} bytes=${Bytes}`);
        // A detached, task-owned checkout is an immutable historical construction
        // input; no second maintained compiler or edition-specific library exists.
        Registered = true;
        await Git('source-checkout', ['worktree', 'add', '--detach', Source, SOURCE_EDITION_PREDECESSOR]);
        const Scripts = path.join(Source, 'Tools', 'Native');
        const Core = await import(pathToFileURL(path.join(Scripts, 'Current-Split-Compiler-Cache-Core.mjs')).href);
        const Runˉnode = (Label, Name, Arguments) => Run(Label, process.execPath,
            [path.join(Scripts, Name), ...Arguments], 1_200_000);
        const Runˉnative = (Label, Name, Arguments) => WINDOWS
            ? Run(Label, path.join(Scripts, Name + '.cmd'), Arguments, 1_200_000)
            : Run(Label, 'bash', [path.join(Scripts, Name + '.sh'), ...Arguments], 1_200_000);
        const Key = await Core.Getˉcurrentˉsplitˉcompilerˉkey();
        const Pair = await Core.Acquireˉcurrentˉsplitˉcompiler(
            await Core.Getˉcurrentˉsplitˉcompilerˉfamily(), Key,
            Candidate => Core.Constructˉcurrentˉsplitˉcompiler(Products, Candidate, Runˉnative, Runˉnode),
            async () => {
                if (await Core.Getˉcurrentˉsplitˉcompilerˉkey() !== Key) {
                    throw new Error('Predecessor compiler inputs changed.');
                }
            },
        );
        console.log(`source-edition predecessor compiler-cache=${Pair.status} key=${Key}`);
        const Result = { Analyzer: path.join(Pair.directory, 'Analyzer' + SUFFIX),
            Analyzerˉidentity: path.join(Pair.directory, 'Analyzer.identity'),
            Emitter: path.join(Pair.directory, 'Emitter' + SUFFIX),
            Emitterˉidentity: path.join(Pair.directory, 'Emitter.identity') };
        const Evidence = [];
        for (const [Name, Project] of [
            ['Reader', 'Windvale-Project-Manifest.wvproj'],
            ['Admitter', 'Windvale-Compiler-Admission-Driver.wvproj'],
            ['Authenticator', 'Windvale-Compiler-Source-Authenticator.wvproj'],
            ['Binder', 'Windvale-Compiler-Foreign-Binding-Driver.wvproj'],
        ]) {
            const Wvb = path.join(Products, Name + '.wvb');
            const Product = path.join(Products, Name + SUFFIX);
            await Runˉnode(Name + '-build', 'Build-Cached-Split-Project-Wvb.mjs', [
                path.join(Source, 'Projects', 'Tools', Project), Wvb,
                Result.Analyzer, Result.Analyzerˉidentity, Result.Emitter, Result.Emitterˉidentity,
                '--symbol-checkpoint',
            ]);
            await Runˉnode(Name + '-package', 'Build-Cached-Segmented-Hosted-Wvb.mjs', ['7', Wvb, Product]);
            Result[Name] = Product;
            const Payload = await readFile(Wvb);
            Evidence.push({ name: Name, bytes: Payload.length,
                sha256: createHash('sha256').update(Payload).digest('hex') });
        }
        await Run('source-unchanged', 'git', ['-C', Source, 'diff', '--exit-code', 'HEAD'], 120_000);
        console.log('source-edition predecessor status=Constructed source=' + SOURCE_EDITION_PREDECESSOR +
            ' products=' + JSON.stringify(Evidence));
        return Result;
    } finally {
        if (Registered) {
            if (path.dirname(Source) !== Work || !path.basename(Source).startsWith('Predecessor-')) {
                throw new Error('Refusing to remove an unexpected predecessor checkout.');
            }
            const Information = await lstat(Source).catch(() => null);
            if (Information !== null && (Information.isSymbolicLink() ||
                !Sameˉpath(await realpath(Source), Source))) {
                throw new Error('Refusing to remove a replaced predecessor checkout.');
            }
            await Git('source-release', ['worktree', 'remove', '--force', Source]);
        }
    }
}

function Sameˉpath(Left, Right) {
    return WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right;
}
