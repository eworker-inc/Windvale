import { mkdtemp, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, extname, join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Getˉcurrentˉsplitˉcompilerˉkey } from './Current-Split-Compiler-Cache-Core.mjs';
import { Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';
import {
    Prepareˉnativeˉprojectˉcacheˉcontext, Getˉnativeˉprojectˉcacheˉrequest,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
} from './Native-Project-Cache-Key-Core.mjs';

const SCRIPT = fileURLToPath(import.meta.url);
const NATIVE = dirname(SCRIPT);
const REPOSITORY = resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const PROJECT = join(REPOSITORY, 'Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj');
const TEMPORARY_PREFIX = 'windvale-current-verify-';
const MAXIMUM_INPUT_BYTES = 16_777_216;
const MAXIMUM_PREPARATION_MILLISECONDS = 600_000;
const MAXIMUM_VERIFICATION_MILLISECONDS = 120_000;

export function Parseˉcurrentˉverification(Arguments, Now = Date.now()) {
    const Usage = () => {
        throw Object.assign(new Error('Usage: Verify-Wvb.mjs --current <module.wvb> | ' +
            '--prepare --deadline-ms <absolute-unix-milliseconds>'), { exitCode: 64 });
    };
    if (!Array.isArray(Arguments) || ![2, 3].includes(Arguments.length) || Arguments.some(Value =>
        typeof Value !== 'string' || Value.length > 32_768)) Usage();
    if (Arguments.length === 2 && Arguments[0] === '--current' &&
        extname(Arguments[1]).toLowerCase() === '.wvb') {
        return { Prepare: false, Input: resolve(Arguments[1]),
            Deadline: Now + MAXIMUM_VERIFICATION_MILLISECONDS };
    }
    if (Arguments.length !== 3 || Arguments[0] !== '--prepare' ||
        Arguments[1] !== '--deadline-ms' || !/^[1-9][0-9]*$/u.test(Arguments[2])) Usage();
    const Deadline = Number(Arguments[2]);
    if (!Number.isSafeInteger(Deadline) || Deadline <= Now ||
        Deadline > Now + MAXIMUM_PREPARATION_MILLISECONDS) Usage();
    return { Prepare: true, Input: null, Deadline };
}

async function Runˉcurrentˉverification(Arguments) {
    const Selection = Parseˉcurrentˉverification(Arguments);
    if (process.arch !== 'x64' || !['win32', 'linux'].includes(process.platform)) {
        throw Object.assign(new Error('Current verification requires Windows or Linux x64.'), { exitCode: 64 });
    }
    const Previousˉmode = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    if (Previousˉmode !== undefined && Previousˉmode !== '1') {
        throw new Error('WINDVALE_PREPARED_PRODUCTS_ONLY must be absent or 1.');
    }
    if (Selection.Prepare && Previousˉmode === '1') {
        throw Object.assign(new Error('Verifier preparation is disabled in prepared-product mode.'), { exitCode: 64 });
    }
    const Input = Selection.Prepare ? null :
        await Readˉboundedˉhostedˉfile(Selection.Input, 'current verifier input', MAXIMUM_INPUT_BYTES);
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext('current-wvb-verification-v1', [SCRIPT,
        join(NATIVE, 'Development-Command-Core.mjs')]);
    const Request = await Getˉnativeˉprojectˉcacheˉrequest(Context, PROJECT);
    const Compilerˉkey = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, TEMPORARY_PREFIX)));
    const Requireˉunchanged = async (Compiler = false) => {
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
        if (Compiler && await Getˉcurrentˉsplitˉcompilerˉkey() !== Compilerˉkey) {
            throw new Error('Current compiler construction inputs changed during verification.');
        }
        if (Input !== null && !Input.equals(await Readˉboundedˉhostedˉfile(
            Selection.Input, 'current verifier input', MAXIMUM_INPUT_BYTES))) {
            throw new Error('Current verifier input changed.');
        }
    };
    async function Run(Step, Tool, Parameters) {
        const Result = await Runˉdevelopmentˉcommand(Tool, Parameters, Selection.Deadline, Selection.Prepare);
        if (Result.Code !== 0 || Result.Error !== '') {
            const Detail = (Result.Error || Result.Output).trim().slice(-4096);
            const Preparation = Selection.Prepare ? '' :
                ' Prepare current verifier products separately with Verify-Wvb.mjs --prepare --deadline-ms <deadline>.';
            throw Object.assign(new Error(`Current verifier ${Step} failed (${Result.Code}): ${Detail}${Preparation}`),
                { exitCode: Result.Code === 64 ? 64 : 1 });
        }
    }
    try {
        // Ordinary verification may only materialize validated existing products.
        // Compiler construction remains disabled in the explicit preparation run.
        if (!Selection.Prepare) process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
        const Wvb = join(Work, 'Verifier.wvb');
        const Verifier = join(Work, WINDOWS ? 'Verifier.exe' : 'Verifier.elf');
        // Bind one current compiler identity across materialization and execution.
        // The child validates that exact checkpoint; this coordinator repeats
        // the complete source identity once before returning the result.
        await Run('source-product', process.execPath, [join(NATIVE, 'Build-Current-Split-Project-Wvb.mjs'),
            '--deadline-ms', String(Selection.Deadline), '--compiler-checkpoint', Compilerˉkey,
            '--prepared-compiler-only', PROJECT, Wvb]);
        await Run('native-product', process.execPath, [join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
            '--deadline-ms', String(Selection.Deadline), '2', Wvb, Verifier]);
        await Requireˉunchanged();
        if (Selection.Prepare) {
            await Requireˉunchanged(true);
            process.stdout.write('current verifier checkpoint status=Prepared compiler-construction=disabled\n');
            return 0;
        }
        const Privateˉinput = join(Work, 'Input.wvb');
        await writeFile(Privateˉinput, Input, { flag: 'wx' });
        const Result = await Runˉdevelopmentˉcommand(Verifier, [Privateˉinput], Selection.Deadline);
        await Requireˉunchanged(true);
        if (!Input.equals(await Readˉboundedˉhostedˉfile(Privateˉinput,
            'private current verifier input', MAXIMUM_INPUT_BYTES))) {
            throw new Error('Private current verifier input changed.');
        }
        if (Result.Code === 0 && Result.Error === '' &&
            Result.Output.replaceAll('\r\n', '\n') === 'wvb status=Valid profile=compiler-aligned\n') {
            process.stdout.write(Result.Output);
            return 0;
        }
        if (Result.Code === 1 && Result.Output === '' &&
            /^wvb status=Invalid phase=[a-z-]+(?: step=[a-z-]+)?\r?\n$/u.test(Result.Error)) {
            process.stderr.write(Result.Error);
            return 1;
        }
        throw new Error('Current verifier returned an invalid result or diagnostic.');
    } finally {
        if (Previousˉmode === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Previousˉmode;
        if (dirname(Work) !== Temporary || !basename(Work).startsWith(TEMPORARY_PREFIX)) {
            throw new Error('Refusing to remove an unowned current-verifier directory.');
        }
        await rm(Work, { recursive: true, force: false });
    }
}

if (process.argv[1] && pathToFileURL(resolve(process.argv[1])).href === import.meta.url) {
    try { process.exitCode = await Runˉcurrentˉverification(process.argv.slice(2)); }
    catch (Error) {
        process.stderr.write(`${Error.message}\n`);
        process.exitCode = Error.exitCode ?? 1;
    }
}
