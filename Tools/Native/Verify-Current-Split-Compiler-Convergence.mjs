import { createHash } from 'node:crypto';
import { mkdtemp, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';
import { Constructˉsourceˉeditionˉpredecessor, SOURCE_EDITION_PREDECESSOR } from './Source-Edition-Predecessor-Core.mjs';
import {
    Prepareˉnativeˉprojectˉcacheˉcontext, Getˉnativeˉprojectˉcacheˉrequest,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
} from './Native-Project-Cache-Key-Core.mjs';
import {
    Acquireˉcurrentˉsplitˉcompiler, Getˉcurrentˉsplitˉcompilerˉfamily, Getˉcurrentˉsplitˉcompilerˉkey,
} from './Current-Split-Compiler-Cache-Core.mjs';

const NATIVE = dirname(fileURLToPath(import.meta.url));
const REPOSITORY = resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_MILLISECONDS = 3_600_000;
const Hash = Bytes => createHash('sha256').update(Bytes).digest('hex');
function Require(Condition, Message) { if (!Condition) throw new Error(Message); }

async function Main() {
    Require(process.argv.length === 3 && process.arch === 'x64' && ['win32', 'linux'].includes(process.platform),
        'Usage: Verify-Current-Split-Compiler-Convergence.mjs <source-root>');
    const Root = await realpath(resolve(process.argv[2]));
    Require(WINDOWS ? Root.toLowerCase() === REPOSITORY.toLowerCase() : Root === REPOSITORY,
        'Convergence requires the active source root.');
    const Key = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext('current-compiler-convergence-v1',
        [fileURLToPath(import.meta.url)]);
    const Verifierˉrequest = await Getˉnativeˉprojectˉcacheˉrequest(Context,
        join(REPOSITORY, 'Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj'));
    const Unchanged = async () => {
        Require(await Getˉcurrentˉsplitˉcompilerˉkey() === Key,
            'Current compiler inputs changed during convergence.');
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Verifierˉrequest);
    };
    const Current = await Acquireˉcurrentˉsplitˉcompiler(await Getˉcurrentˉsplitˉcompilerˉfamily(), Key,
        () => { throw new Error('Prepare the current compiler checkpoint in a separately selected construction run.'); }, Unchanged);
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, 'windvale-current-split-convergence-')));
    const Started = Date.now();
    const Deadline = Started + MAXIMUM_MILLISECONDS;
    let Phase = 0;
    let Complete = false;
    async function Run(Label, Command, Arguments, Rejection = false) {
        const Item = ++Phase;
        process.stdout.write('START native compiler convergence phase=' + Item + '/13 step=' + Label + '\n');
        const Result = await Runˉdevelopmentˉcommand(Command, Arguments,
            Math.min(Deadline, Date.now() + 900_000), true, 1_048_576);
        Require(Result.Error === '' && (Rejection ? Result.Code === 1 &&
            Result.Output.includes('wvb status=Invalid') : Result.Code === 0),
            Label + ' failed exit=' + Result.Code + ': ' + Result.Output + Result.Error);
        process.stdout.write('PASS native compiler convergence phase=' + Item + '/13 step=' + Label + '\n');
    }
    async function Node(Label, Name, Arguments) {
        await Run(Label, process.execPath, [join(NATIVE, Name), ...Arguments]);
    }
    async function Build(Label, Project, Output, Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity, Admission, Generation) {
        const Cache = join(Work, 'Emission-' + Generation);
        // Separate empty generations prevent a byte-identical producer identity
        // from turning the second reconstruction into a first-generation hit.
        const Code = "import {pathToFileURL} from 'node:url';process.env.WINDVALE_NATIVE_CACHE_ROOT=" +
            JSON.stringify(Cache) + ";await import(pathToFileURL(process.argv[1]).href);";
        await Run(Label, process.execPath, ['--input-type=module', '-e', Code,
            join(NATIVE, 'Build-Cached-Split-Project-Wvb.mjs'),
            join(REPOSITORY, 'Projects/Tools', Project), Output,
            Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity, '--authenticated-project4',
            ...['Admitter', 'Authenticator', 'Reader', 'Binder'].map(Name => Admission[Name])]);
    }
    try {
        const Frozen = await Constructˉsourceˉeditionˉpredecessor(Work);
        const Admission = Object.fromEntries(['Admitter', 'Authenticator', 'Reader', 'Binder']
            .map(Name => [Name, join(Current.directory, Name + SUFFIX)]));
        const Analyzer1 = join(Work, 'Analyzer-Stage1.wvb');
        const Emitter1 = join(Work, 'Emitter-Stage1.wvb');
        const Analyzer2 = join(Work, 'Analyzer-Stage2.wvb');
        const Emitter2 = join(Work, 'Emitter-Stage2.wvb');
        const Analyzer = join(Work, 'Analyzer' + SUFFIX);
        const Emitter = join(Work, 'Emitter' + SUFFIX);
        const Analyzerˉidentity = join(Work, 'Analyzer.identity');
        const Emitterˉidentity = join(Work, 'Emitter.identity');
        await Build('analyzer-stage1', 'Windvale-Compiler-Analysis-Driver.wvproj', Analyzer1,
            Frozen.Analyzer, Frozen.Analyzerˉidentity, Frozen.Emitter, Frozen.Emitterˉidentity, Frozen, 1);
        await Node('analyzer-package', 'Build-Cached-Segmented-Hosted-Wvb.mjs', ['8', Analyzer1, Analyzer]);
        await Node('analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', ['analyzer', Analyzer, Analyzerˉidentity]);
        await Build('emitter-stage1', 'Windvale-Compiler-Emission-Driver.wvproj', Emitter1,
            Analyzer, Analyzerˉidentity, Frozen.Emitter, Frozen.Emitterˉidentity, Frozen, 1);
        await Node('emitter-package', 'Build-Cached-Segmented-Hosted-Wvb.mjs', ['8', Emitter1, Emitter]);
        await Node('emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', ['emitter', Emitter, Emitterˉidentity]);
        await Build('analyzer-stage2', 'Windvale-Compiler-Analysis-Driver.wvproj', Analyzer2,
            Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity, Admission, 2);
        await Build('emitter-stage2', 'Windvale-Compiler-Emission-Driver.wvproj', Emitter2,
            Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity, Admission, 2);
        const Products = [];
        for (const [Name, First, Second] of [['analyzer', Analyzer1, Analyzer2], ['emitter', Emitter1, Emitter2]]) {
            const Left = await Readˉboundedˉhostedˉfile(First, Name + ' first generation', MAXIMUM_WVB_BYTES);
            const Right = await Readˉboundedˉhostedˉfile(Second, Name + ' second generation', MAXIMUM_WVB_BYTES);
            Require(Left.equals(Right), Name + ' failed byte-identical source convergence.');
            Products.push({ name: Name, bytes: Left.length, sha256: Hash(Left) });
        }
        const Verifierˉwvb = join(Work, 'Verifier.wvb');
        const Verifier = join(Work, 'Verifier' + SUFFIX);
        await Build('verifier-build', 'Windvale-Compiler-Wvb-Verifier.wvproj', Verifierˉwvb,
            Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity, Admission, 2);
        await Node('verifier-package', 'Build-Cached-Segmented-Hosted-Wvb.mjs', ['7', Verifierˉwvb, Verifier]);
        await Run('analyzer-verification', Verifier, [Analyzer2]);
        await Run('emitter-verification', Verifier, [Emitter2]);
        const Malformed = join(Work, 'Malformed.wvb');
        const Bytes = Buffer.from(await Readˉboundedˉhostedˉfile(Analyzer2, 'malformed-input oracle', MAXIMUM_WVB_BYTES));
        Bytes[0] ^= 1;
        await writeFile(Malformed, Bytes, { flag: 'wx' });
        await Run('verifier-rejection', Verifier, [Malformed], true);
        await Unchanged();
        Complete = true;
        process.stdout.write('native compiler convergence status=Complete products=2 host=' +
            process.platform + '-' + process.arch + ' source=' + SOURCE_EDITION_PREDECESSOR +
            ' current-key=' + Key + ' generations=Fresh-Isolated verifier-profile=7 packaging-cache=Reusable elapsed-ms=' +
            (Date.now() - Started) + ' products=' + JSON.stringify(Products) + '\n');
    } finally {
        Require(dirname(Work) === Temporary && basename(Work).startsWith('windvale-current-split-convergence-'),
            'Refusing to remove an unowned convergence directory.');
        if (Complete) await rm(Work, { recursive: true, force: false });
        else process.stderr.write('Convergence incomplete; retained phase artifacts: ' + Work + '\n');
    }
}
try { await Main(); }
catch (Error) {
    process.stderr.write(Error.message + '\n');
    process.exitCode = Error.exitCode ?? 1;
}
