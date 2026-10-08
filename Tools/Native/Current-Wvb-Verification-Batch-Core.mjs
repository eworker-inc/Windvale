import { mkdtemp, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, extname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createHash } from 'node:crypto';
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
const MAXIMUM_VERIFICATION_MILLISECONDS = 120_000;
const MAXIMUM_BATCH_INPUTS = 1_024;
const PRODUCERS = [SCRIPT, join(NATIVE, 'Verify-Wvb.mjs'), join(NATIVE, 'Development-Command-Core.mjs')];
const LOADED = await Promise.all(PRODUCERS.map(async Path => ({ Path,
    Sha256: Digest(await Readˉboundedˉhostedˉfile(Path, 'loaded current-verifier batch producer')) })));
let Batchˉactive = false;

function Digest(Bytes) { return createHash('sha256').update(Bytes).digest('hex'); }

// The bounded scope owns one existing current verifier. Each input is freshly
// admitted; complete source/construction identity is checked at both boundaries.
export async function Withˉcurrentˉverification(Selection, Use) {
    if (Batchˉactive) throw new Error('A current verification batch is already active in this process.');
    // Materialization temporarily selects process-wide prepared-product mode.
    // Refuse overlap before any await, including failures during scope opening.
    Batchˉactive = true;
    try { return await Runˉbatch(Selection, Use); }
    finally { Batchˉactive = false; }
}

async function Runˉbatch(Selection, Use) {
    if (Selection?.Prepare !== false || !Number.isSafeInteger(Selection.Deadline) ||
        Selection.Deadline <= Date.now() || Selection.Deadline > Date.now() + 7_200_000 ||
        typeof Use !== 'function') throw new Error('Invalid current verification batch.');
    if (process.arch !== 'x64' || !['win32', 'linux'].includes(process.platform)) {
        throw Object.assign(new Error('Current verification requires Windows or Linux x64.'), { exitCode: 64 });
    }
    const Previousˉmode = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    if (Previousˉmode !== undefined && Previousˉmode !== '1') {
        throw new Error('WINDVALE_PREPARED_PRODUCTS_ONLY must be absent or 1.');
    }
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext('current-wvb-verification-batch-v1', PRODUCERS);
    const Request = await Getˉnativeˉprojectˉcacheˉrequest(Context, PROJECT);
    const Compilerˉkey = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Requireˉunchanged = async (Compiler = false) => {
        for (const Producer of LOADED) {
            if (Digest(await Readˉboundedˉhostedˉfile(Producer.Path, 'loaded current-verifier batch producer')) !== Producer.Sha256)
                throw new Error('Loaded current-verifier batch producer changed.');
        }
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
        if (Compiler && await Getˉcurrentˉsplitˉcompilerˉkey() !== Compilerˉkey) {
            throw new Error('Current compiler construction inputs changed during verification.');
        }
    };
    await Requireˉunchanged();
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, TEMPORARY_PREFIX)));
    async function Run(Step, Tool, Parameters) {
        const Result = await Runˉdevelopmentˉcommand(Tool, Parameters, Selection.Deadline);
        if (Result.Code !== 0 || Result.Error !== '') {
            const Detail = (Result.Error || Result.Output).trim().slice(-4096);
            throw Object.assign(new Error(`Current verifier ${Step} failed (${Result.Code}): ${Detail}` +
                ' Prepare current verifier products separately with Verify-Wvb.mjs --prepare --deadline-ms <deadline>.'),
                { exitCode: Result.Code === 64 ? 64 : 1 });
        }
    }
    let Active = false, Busy = false, Count = 0, Imageˉsha256 = null, Pending = null;
    const Verifier = join(Work, WINDOWS ? 'Verifier.exe' : 'Verifier.elf');
    const Imageˉidentity = async () => Digest(
        await Readˉboundedˉhostedˉfile(Verifier, 'current verifier executable', 134_217_728, false, true));
    const Requireˉimage = async () => {
        if (Date.now() >= Selection.Deadline) throw Object.assign(new Error('Current verification deadline expired.'), { exitCode: 124 });
        if (await Imageˉidentity() !== Imageˉsha256) throw new Error('Current verifier executable changed.');
    };
    function Verify(Inputˉpath) {
        if (!Active || Busy || Count === MAXIMUM_BATCH_INPUTS || typeof Inputˉpath !== 'string' ||
            extname(Inputˉpath).toLowerCase() !== '.wvb') throw new Error('Current verification batch is closed, busy or exceeds its input bound.');
        Busy = true;
        const Privateˉinput = join(Work, 'Input-' + (++Count) + '.wvb');
        let Created = false;
        Pending = (async () => {
            try {
                const Input = await Readˉboundedˉhostedˉfile(Inputˉpath, 'current verifier input', MAXIMUM_INPUT_BYTES);
                await Requireˉimage();
                await writeFile(Privateˉinput, Input, { flag: 'wx' }); Created = true;
                const Result = await Runˉdevelopmentˉcommand(Verifier, [Privateˉinput],
                    Math.min(Selection.Deadline, Date.now() + MAXIMUM_VERIFICATION_MILLISECONDS));
                await Requireˉimage();
                if (!Input.equals(await Readˉboundedˉhostedˉfile(Inputˉpath, 'current verifier input', MAXIMUM_INPUT_BYTES)))
                    throw new Error('Current verifier input changed.');
                if (!Input.equals(await Readˉboundedˉhostedˉfile(Privateˉinput, 'private current verifier input', MAXIMUM_INPUT_BYTES)))
                    throw new Error('Private current verifier input changed.');
                if ((Result.Code === 0 && Result.Error === '' &&
                    Result.Output.replaceAll('\r\n', '\n') === 'wvb status=Valid profile=compiler-aligned\n') ||
                    (Result.Code === 1 && Result.Output === '' &&
                    /^wvb status=Invalid phase=[a-z-]+(?: step=[a-z-]+)?\r?\n$/u.test(Result.Error))) return Result;
                throw new Error('Current verifier returned an invalid result or diagnostic.');
            } finally {
                if (Created) await rm(Privateˉinput, { force: false });
                Busy = false;
            }
        })();
        return Pending;
    }
    try {
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
        const Wvb = join(Work, 'Verifier.wvb');
        await Run('source-product', process.execPath, [join(NATIVE, 'Build-Current-Split-Project-Wvb.mjs'),
            '--deadline-ms', String(Selection.Deadline), '--compiler-checkpoint', Compilerˉkey,
            '--prepared-compiler-only', PROJECT, Wvb]);
        await Run('native-product', process.execPath, [join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
            '--deadline-ms', String(Selection.Deadline), '2', Wvb, Verifier]);
        await Requireˉunchanged();
        Imageˉsha256 = await Imageˉidentity(); Active = true;
        // The caller may prepare its application products; only verifier
        // materialization forces read-only prepared-product mode.
        if (Previousˉmode === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Previousˉmode;
        const Value = await Use(Object.freeze({ Verify }));
        if (Busy) throw new Error('Current verification callback returned with an unfinished input.');
        return Value;
    } finally {
        Active = false;
        try {
            try { if (Pending !== null) await Pending; }
            finally { if (Imageˉsha256 !== null) { await Requireˉimage(); await Requireˉunchanged(true); } }
        } finally {
            if (Previousˉmode === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
            else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Previousˉmode;
            if (dirname(Work) !== Temporary || !basename(Work).startsWith(TEMPORARY_PREFIX)) {
                throw new Error('Refusing to remove an unowned current-verifier directory.');
            }
            await rm(Work, { recursive: true, force: false });
        }
    }
}
