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
const CLEANUP_RESERVE_MILLISECONDS = 30_000;
// Development commands may need two seconds to stop a Windows process tree
// and another five seconds to settle its streams after their deadline.
const COMMAND_SETTLEMENT_MILLISECONDS = 7_500;
const WINDOWS = process.platform === 'win32';
const SUFFIX = WINDOWS ? '.exe' : '.elf';

// Keep a child timeout or framework failure distinguishable from a failed build.
export async function Runˉcompilerˉconstructionˉcommand(
    Label, Command, Arguments, Deadline, Stream = true, Execute = Runˉdevelopmentˉcommand,
    Allowˉstderr = false,
) {
    let Result;
    try {
        Result = await Execute(Command, Arguments, Deadline, Stream, 1_048_576);
    } catch (Failure) {
        const Wrapped = Failure instanceof Error ? Failure : new Error(String(Failure));
        if (!Number.isInteger(Wrapped.exitCode) || Wrapped.exitCode < 1 || Wrapped.exitCode > 255) {
            Wrapped.exitCode = 2;
        }
        if (Wrapped.exitCode === 2) Wrapped.cleanupUncertain = true;
        throw Wrapped;
    }
    if (Result === null || typeof Result !== 'object' ||
        !Number.isInteger(Result.Code) || Result.Code < 0 || Result.Code > 255 ||
        typeof Result.Output !== 'string' || typeof Result.Error !== 'string') {
        throw Object.assign(new Error(`${Label} returned an invalid construction process result.`),
            { exitCode: 2, cleanupUncertain: true });
    }
    if (Result.Code !== 0 || (!Allowˉstderr && Result.Error !== '')) {
        const Code = Result.Code === 0 ? 2 : Result.Code;
        throw Object.assign(new Error(`${Label} failed: status=${Result.Code}.\n${Result.Error || Result.Output}`),
            { exitCode: Code, ...(Code === 2 ? { cleanupUncertain: true } : {}) });
    }
    return Result.Output;
}

export function Hasˉuncertainˉconstructionˉcleanup(Failure) {
    const Pending = [Failure];
    const Seen = new Set();
    while (Pending.length !== 0 && Seen.size < 32) {
        const Value = Pending.shift();
        if (Seen.has(Value)) continue;
        Seen.add(Value);
        if (Value === null || typeof Value !== 'object') continue;
        if (Value.cleanupUncertain === true) return true;
        if (Value instanceof AggregateError) {
            if (Value.errors.length > 8) return true;
            Pending.push(...Value.errors);
        }
        if (Value.cleanupFailure !== undefined) Pending.push(Value.cleanupFailure);
        if (Value.cause !== undefined) Pending.push(Value.cause);
    }
    return Pending.length !== 0;
}

export async function Constructˉsourceˉeditionˉpredecessor(Work, Deadline = null) {
    if (Deadline !== null && (!Number.isSafeInteger(Deadline) || Deadline <= 0)) {
        throw new Error('Invalid source-edition predecessor deadline.');
    }
    const Workˉdeadline = Deadline === null ? null : Deadline - CLEANUP_RESERVE_MILLISECONDS;
    Requireˉtime(Workˉdeadline, 'construction');
    Work = path.resolve(Work);
    if (!Sameˉpath(await realpath(Work), Work) || !(await lstat(Work)).isDirectory()) {
        throw new Error('The predecessor work directory must be canonical.');
    }
    const Source = path.join(Work, 'Predecessor-' + randomBytes(12).toString('hex'));
    const Products = path.join(Work, 'Predecessor-Products');
    Requireˉtime(Workˉdeadline, 'product-directory creation');
    await mkdir(Products);
    let Registered = false;
    let Primaryˉfailure = null;
    async function Run(Label, Command, Arguments, Maximum = 600_000, Stream = true, Cleanup = false) {
        const Boundary = Deadline === null ? null : Cleanup
            ? Deadline - COMMAND_SETTLEMENT_MILLISECONDS : Workˉdeadline;
        Requireˉtime(Boundary, Label);
        console.log(`source-edition predecessor step=${Label} status=Started`);
        // Git writes successful worktree progress to stderr; other producers do not.
        const Output = await Runˉcompilerˉconstructionˉcommand('Predecessor ' + Label, Command, Arguments,
            Boundary === null ? Date.now() + Maximum : Math.min(Date.now() + Maximum, Boundary),
            Stream, Runˉdevelopmentˉcommand, Command === 'git');
        Requireˉtime(Cleanup ? Deadline : Boundary, Label);
        console.log(`source-edition predecessor step=${Label} status=Complete`);
        return Output;
    }
    const Git = (Label, Arguments, Cleanup = false) => Run(Label, 'git', ['-C', REPOSITORY_ROOT, ...Arguments],
        120_000, Label !== 'source-inventory', Cleanup);
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
                Requireˉtime(Workˉdeadline, 'compiler identity check');
                if (await Core.Getˉcurrentˉsplitˉcompilerˉkey() !== Key) {
                    throw new Error('Predecessor compiler inputs changed.');
                }
                Requireˉtime(Workˉdeadline, 'compiler identity check');
            },
        );
        Requireˉtime(Workˉdeadline, 'compiler construction');
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
        Requireˉtime(Workˉdeadline, 'publication');
        console.log('source-edition predecessor status=Constructed source=' + SOURCE_EDITION_PREDECESSOR +
            ' products=' + JSON.stringify(Evidence));
        return Result;
    } catch (Failure) {
        Primaryˉfailure = Failure instanceof Error ? Failure : new Error(String(Failure));
        throw Primaryˉfailure;
    } finally {
        if (Registered && Hasˉuncertainˉconstructionˉcleanup(Primaryˉfailure)) {
            // A possibly live historical producer may still use this checkout.
            // Preserve both its files and Git registration for explicit recovery.
            Primaryˉfailure.predecessorCheckout = Source;
            Primaryˉfailure.cleanupUncertain = true;
            Primaryˉfailure.cleanupFailure = Object.assign(new Error(
                `Predecessor checkout preserved at ${Source}: process termination is unproven.`),
            { exitCode: 2, cleanupUncertain: true });
        } else if (Registered) {
            try {
                if (path.dirname(Source) !== Work || !path.basename(Source).startsWith('Predecessor-')) {
                    throw new Error('Refusing to remove an unexpected predecessor checkout.');
                }
                const Information = await lstat(Source).catch(() => null);
                if (Information !== null && (Information.isSymbolicLink() ||
                    !Sameˉpath(await realpath(Source), Source))) {
                    throw new Error('Refusing to remove a replaced predecessor checkout.');
                }
                await Git('source-release', ['worktree', 'remove', '--force', Source], true);
            } catch (Failure) {
                const Cleanupˉfailure = Failure instanceof Error ? Failure : new Error(String(Failure));
                Cleanupˉfailure.predecessorCheckout = Source;
                Cleanupˉfailure.message = `Predecessor checkout preserved at ${Source}: ${Cleanupˉfailure.message}`;
                if (Primaryˉfailure === null) throw Cleanupˉfailure;
                Primaryˉfailure.predecessorCheckout = Source;
                Primaryˉfailure.cleanupFailure = Cleanupˉfailure;
            }
        }
    }
}

function Requireˉtime(Boundary, Phase) {
    if (Boundary !== null && Date.now() >= Boundary) {
        throw Object.assign(new Error(`Source-edition predecessor deadline expired before ${Phase}.`), { exitCode: 124 });
    }
}

function Sameˉpath(Left, Right) {
    return WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right;
}
