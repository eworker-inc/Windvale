import { lstat, mkdtemp, readFile, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    Getˉcurrentˉsplitˉcompilerˉfamily,
    Getˉcurrentˉsplitˉcompilerˉkey,
    Readˉpreparedˉsplitˉcompiler,
} from './Current-Split-Compiler-Cache-Core.mjs';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';

const NATIVE = dirname(fileURLToPath(import.meta.url));
const REPOSITORY = resolve(NATIVE, '..', '..');
const MAXIMUM_MILLISECONDS = 240_000;
const CLEANUP_RESERVE_MILLISECONDS = 5_000;
const MAXIMUM_MANIFEST_BYTES = 65_536;
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const TEMPORARY_PREFIX = 'windvale-foundation-values-';
const CASES = Object.freeze([
    ['conformance', 'Language-1.0-Foundation-Generic-Result-Project4', null],
    ['conformance', 'Language-1.0-Foundation-Memory-Limit-Failure', null],
    ['negative', 'Language-1.0-Foundation-Memory-Limit-Failure-Wrong-Field', 'Typeˉmismatch'],
    ['negative', 'Language-1.0-Foundation-Memory-Limit-Failure-Lookalike', 'Invalidˉargument'],
].map(([Kind, Name, Diagnostic]) => Object.freeze({
    Kind, Name, Diagnostic, Project: `Projects/Tests/${Name}.wvproj`,
})));

function Reject(Message) { throw new Error(Message); }

function Requireˉtime(Deadline) {
    if (Date.now() >= Deadline) {
        throw Object.assign(new Error('Foundation value conformance exceeded its shared 240-second deadline.'),
            { exitCode: 124 });
    }
}

async function Readˉordinary(Candidate, Maximum) {
    const Information = await lstat(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum) {
        Reject(`Foundation value input must be a bounded ordinary file: ${Candidate}`);
    }
    const Value = await readFile(Candidate);
    if (Value.length !== Information.size) Reject('Foundation value input changed while read.');
    return Value;
}

async function Readˉcases() {
    const Value = await Readˉordinary(join(REPOSITORY, 'Tests/Native/Library-Development-Targets.txt'),
        MAXIMUM_MANIFEST_BYTES);
    const Text = new TextDecoder('utf-8', { fatal: true }).decode(Value);
    if (!Text.endsWith('\n') || Text.includes('\r')) Reject('Library development targets must use LF text.');
    const [Header, ...Lines] = Text.trimEnd().split('\n');
    if (Header !== 'windvale-library-development-targets 1') Reject('Invalid library development-target manifest.');
    const Entries = Lines.filter(Line => Line.startsWith('foundation-values|'));
    if (Entries.length !== CASES.length || CASES.some((Case, Index) =>
        Entries[Index] !== `foundation-values|${Case.Kind}|${Case.Project}`)) {
        Reject('Foundation value target must declare its exact two conformance and two negative cases.');
    }
    for (const Case of CASES) {
        const Project = await Readˉordinary(join(REPOSITORY, Case.Project), MAXIMUM_MANIFEST_BYTES);
        if (Project.toString('utf8').split(/\r?\n/u)[0] !== 'windvale-project 4') {
            Reject(`Foundation value conformance requires Project 4: ${Case.Project}`);
        }
    }
    return Value;
}

async function Exists(Candidate) {
    try { await lstat(Candidate); return true; }
    catch (Error) { if (Error.code === 'ENOENT') return false; throw Error; }
}

async function Requireˉwvbˉheader(Candidate) {
    const Value = await Readˉordinary(Candidate, MAXIMUM_WVB_BYTES);
    if (Value.length < 12 || Value.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Value.readUInt16LE(4) !== 1 || Value.readUInt16LE(6) < 11 ||
        Value.readUInt32LE(8) !== 7) Reject('Foundation value WVB header is invalid.');
    var Cursor = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Value.length - Cursor < 8 || Value[Cursor] !== Kind || Value[Cursor + 1] !== 0 ||
            Value.readUInt16LE(Cursor + 2) !== 0) Reject('Foundation value WVB section header is invalid.');
        const Length = Value.readUInt32LE(Cursor + 4);
        if (Length > Value.length - Cursor - 8) Reject('Foundation value WVB section exceeds its input.');
        Cursor += 8 + Length;
    }
    if (Cursor !== Value.length) Reject('Foundation value WVB has trailing bytes.');
}

async function Main() {
    if (process.argv.length !== 2 || process.arch !== 'x64' ||
        !['win32', 'linux'].includes(process.platform)) {
        throw Object.assign(new Error('Foundation value cases require Windows or Linux x64 and no arguments.'),
            { exitCode: 64 });
    }
    // Prepared products forbid fresh analysis, including the expected rejections.
    // This owner uses prepared compiler tools to construct only its test modules.
    if (process.env.WINDVALE_PREPARED_PRODUCTS_ONLY !== undefined) {
        throw Object.assign(new Error('Foundation value cases require fresh source analysis with prepared compiler tools. ' +
            'Unset WINDVALE_PREPARED_PRODUCTS_ONLY; compiler and publisher construction remain disabled.'),
        { exitCode: 64 });
    }
    const Deadline = Date.now() + MAXIMUM_MILLISECONDS;
    const Workˉdeadline = Deadline - CLEANUP_RESERVE_MILLISECONDS;
    const Targetˉmanifest = await Readˉcases();
    Requireˉtime(Workˉdeadline);
    process.stdout.write('native libraries foundation step=compiler-preflight maximum-seconds=240 construction=disabled\n');
    const Compilerˉkey = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Compiler = await Readˉpreparedˉsplitˉcompiler(
        await Getˉcurrentˉsplitˉcompilerˉfamily(), Compilerˉkey);
    Requireˉtime(Workˉdeadline);
    process.stdout.write(`native libraries foundation compiler-cache=Hit key=${Compilerˉkey} host=${process.platform}-${process.arch}\n`);
    const Suffix = process.platform === 'win32' ? '.exe' : '.elf';
    const Product = Name => join(Compiler.directory, Name + Suffix);
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, TEMPORARY_PREFIX)));
    const Workˉidentity = await lstat(Work);
    let Preserveˉwork = false;
    try {
        for (const [Index, Case] of CASES.entries()) {
            Requireˉtime(Workˉdeadline);
            const Started = Date.now();
            process.stdout.write(`native libraries foundation step=source item=${Index + 1}/${CASES.length} ` +
                `kind=${Case.Kind} project=${Case.Project}\n`);
            const Output = join(Work, `Case-${Index + 1}.wvb`);
            const Result = await Runˉdevelopmentˉcommand(process.execPath, [
                join(NATIVE, 'Run-Split-Compiler.mjs'), Product('Admitter'), Product('Authenticator'),
                Product('Analyzer'), Product('Emitter'), '--foreign-binder', Product('Binder'),
                '--workspace', join(REPOSITORY, 'Windvale.wvws'),
                '--project', join(REPOSITORY, Case.Project), '--manifest-reader', Product('Reader'), Output,
            ], Workˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
            if (/split compiler status=(?:Terminationˉfailure|Cleanupˉfailure)/u.test(Result.Error)) {
                Preserveˉwork = true;
                Reject(`Foundation value case did not prove cleanup: ${Result.Error.trim()}`);
            }
            if (Case.Diagnostic === null) {
                if (Result.Code !== 0 || Result.Error !== '') {
                    Reject(`Foundation value source build failed (${Result.Code}): ${Result.Error.trim()}`);
                }
                await Requireˉwvbˉheader(Output);
                process.stdout.write(`native libraries foundation step=canonical-verifier item=${Index + 1}/${CASES.length}\n`);
                const Verified = await Runˉdevelopmentˉcommand(process.execPath,
                    [join(NATIVE, 'Verify-Wvb.mjs'), '--current', Output], Workˉdeadline, false,
                    MAXIMUM_DIAGNOSTIC_BYTES);
                const Report = Verified.Output.replaceAll('\r\n', '\n').replace(
                    /^INFO development command step=active tool=(?:node|Verifier\.(?:exe|elf))\n/gmu, '');
                if (Verified.Code !== 0 || Verified.Error !== '' ||
                    Report !== 'wvb status=Valid profile=compiler-aligned\n') {
                    Reject(`Foundation value canonical verification failed (${Verified.Code}): ` +
                        (Verified.Error || Verified.Output).trim());
                }
            } else {
                const Pattern = new RegExp('^source analysis status=Sourceˉwir symbol-status=Valid ' +
                    'binding-status=Valid wir-status=' + Case.Diagnostic +
                    ' failure-module=0 related-module=[0-9]+ function=[0-9]+ offset=[0-9]+ ' +
                    'line=[1-9][0-9]* column=[1-9][0-9]*\\r?\\n?$', 'u');
                if (Result.Code !== 1 || await Exists(Output) || !Pattern.test(Result.Error)) {
                    Reject(`Foundation value negative case did not report ${Case.Diagnostic} without output: ` +
                        `exit=${Result.Code} ${Result.Error.trim()}`);
                }
            }
            Requireˉtime(Workˉdeadline);
            process.stdout.write(`native libraries foundation item=${Index + 1}/${CASES.length} ` +
                `status=Passed elapsed-ms=${Date.now() - Started}\n`);
        }
        await Compiler.Requireˉunchanged();
        if (await Getˉcurrentˉsplitˉcompilerˉkey() !== Compilerˉkey ||
            !(await Readˉcases()).equals(Targetˉmanifest)) Reject('Foundation value inputs changed during verification.');
        Requireˉtime(Workˉdeadline);
    } catch (Error) {
        Preserveˉwork ||= Error.cleanupUncertain === true;
        throw Error;
    } finally {
        const Current = await lstat(Work);
        if (dirname(Work) !== Temporary || !basename(Work).startsWith(TEMPORARY_PREFIX) ||
            Current.isSymbolicLink() || !Current.isDirectory() ||
            Current.dev !== Workˉidentity.dev || Current.ino !== Workˉidentity.ino ||
            await realpath(Work) !== Work) Reject('Refusing to remove an unowned Foundation value directory.');
        if (Preserveˉwork) process.stderr.write(`Foundation value work preserved after uncertain cleanup: ${Work}\n`);
        else await rm(Work, { recursive: true, force: false });
    }
    Requireˉtime(Deadline);
    process.stdout.write('native libraries development status=Passed target=foundation-values ' +
        'projects=0 conformance-builds=2 negative=2 cases=4\n');
}

try { await Main(); }
catch (Error) {
    process.stderr.write(`${String(Error.message).slice(0, 4096)}\n`);
    process.exitCode = Error.exitCode ?? 1;
}
