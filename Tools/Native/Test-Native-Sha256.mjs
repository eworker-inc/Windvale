import { spawn } from 'node:child_process';
import { createHash } from 'node:crypto';
import { copyFile, mkdtemp, open, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, join, resolve } from 'node:path';

const WINDOWS = process.platform === 'win32';
const OUTPUT_LIMIT = 1024 * 1024;
const COMMAND_TIMEOUT_MILLISECONDS = 120_000;
const CONSTRUCTION_TIMEOUT_MILLISECONDS = 20 * 60_000;
const TASKKILL_TIMEOUT_MILLISECONDS = 2_000;
const TERMINATION_SETTLE_MILLISECONDS = 5_000;
const PROGRESS_INTERVAL_MILLISECONDS = 30_000;
const MAXIMUM_WVO_BYTES = 8 * 1024 * 1024;
const MAXIMUM_MANIFEST_BYTES = 64 * 1024;
const MAXIMUM_STAGING_CHUNK_BYTES = 4 * 1024 * 1024;
const MAXIMUM_APPLICATION_BYTES = 64 * 1024 * 1024;
const HELPER_BYTES = 1640;
const CONTEXT_BYTES = 112;
let Runˉdeadline = Infinity;

function Reject(Message) {
    throw new Error(Message);
}

function Equal(Left, Right, Message) {
    if (!Left.equals(Right)) Reject(Message);
}

function U32(Bytes, Offset, Label) {
    if (Offset < 0 || Offset > Bytes.length || 4 > Bytes.length - Offset) {
        Reject(`Truncated ${Label}.`);
    }
    return Bytes.readUInt32LE(Offset);
}

async function Readˉbounded(Path, Maximumˉbytes, Label) {
    const Handle = await open(Path, 'r');
    try {
        const Information = await Handle.stat();
        const Size = Information.size;
        if (!Information.isFile() || !Number.isSafeInteger(Size) || Size < 0 ||
            Size > Maximumˉbytes) {
            Reject(`${Label} exceeds its ${Maximumˉbytes}-byte file bound.`);
        }
        const Result = Buffer.alloc(Size);
        let Position = 0;
        while (Position < Size) {
            const Readˉresult = await Handle.read(Result, Position, Size - Position, Position);
            const Bytesˉread = Readˉresult.bytesRead;
            if (Bytesˉread === 0) Reject(`${Label} changed while it was read.`);
            Position += Bytesˉread;
        }
        const Probe = Buffer.alloc(1);
        const Probeˉresult = await Handle.read(Probe, 0, 1, Size);
        const Probeˉbytes = Probeˉresult.bytesRead;
        if (Probeˉbytes !== 0) Reject(`${Label} grew while it was read.`);
        return Result;
    } finally {
        await Handle.close();
    }
}

function Processˉisˉlive(Child) {
    return Child.pid !== undefined && Child.exitCode === null && Child.signalCode === null;
}

function Runˉboundedˉtaskkill(Processˉidentifier) {
    return new Promise(Resolveˉresult => {
        const Killer = spawn(
            'taskkill.exe', ['/pid', String(Processˉidentifier), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true }
        );
        let Settled = false;
        const Timer = setTimeout(() => {
            if (Settled) return;
            Settled = true;
            Killer.kill('SIGKILL');
            Killer.unref();
            Resolveˉresult(
                `taskkill did not settle within ${TASKKILL_TIMEOUT_MILLISECONDS} ms`
            );
        }, TASKKILL_TIMEOUT_MILLISECONDS);
        Killer.once('error', Errorˉvalue => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolveˉresult(`taskkill error: ${Errorˉvalue.message}`);
        });
        Killer.once('close', Code => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolveˉresult(Code === 0 ? null : `taskkill exited ${Code}`);
        });
    });
}

async function Terminateˉprocessˉtree(Child) {
    if (!Processˉisˉlive(Child)) return null;
    let Diagnostic = null;
    if (WINDOWS) {
        Diagnostic = await Runˉboundedˉtaskkill(Child.pid);
    } else {
        try {
            process.kill(-Child.pid, 'SIGKILL');
        } catch (Errorˉvalue) {
            Diagnostic = `process-group kill error: ${Errorˉvalue.message}`;
        }
    }
    if (Processˉisˉlive(Child)) {
        try {
            if (!Child.kill('SIGKILL') && Diagnostic !== null) {
                Diagnostic += '; direct child kill returned false';
            }
        } catch (Errorˉvalue) {
            Diagnostic = Diagnostic === null
                ? `direct child kill error: ${Errorˉvalue.message}`
                : `${Diagnostic}; direct child kill error: ${Errorˉvalue.message}`;
        }
    }
    return Diagnostic;
}

async function Runˉprocess(Command, Arguments, Options = {}) {
    const Remaining = Runˉdeadline - Date.now();
    if (Remaining <= 0) throw Object.assign(new Error(
        'Native SHA-256 total budget expired.'), { exitCode: 124 });
    const Timeoutˉmilliseconds = Math.min(Remaining,
        Options.Timeoutˉmilliseconds ?? COMMAND_TIMEOUT_MILLISECONDS);
    const Isˉcommand = WINDOWS && Command.toLowerCase().endsWith('.cmd');
    if (Isˉcommand && [Command, ...Arguments].some(
        Argument => /[\r\n&|<>^%!"]/u.test(Argument))) {
        Reject('A Windows native-test argument contains shell metacharacters.');
    }
    const Executable = Isˉcommand ? process.env.ComSpec ?? 'cmd.exe' : Command;
    const Producerˉarguments = Isˉcommand
        ? [
            '/d', '/v:off', '/s', '/c',
            `"${[Command, ...Arguments].map(Argument => `"${Argument}"`).join(' ')}"`,
        ]
        : Arguments;
    const Child = spawn(Executable, Producerˉarguments, {
        cwd: Options.Workingˉdirectory,
        detached: !WINDOWS,
        windowsHide: true,
        stdio: ['ignore', 'pipe', 'pipe'],
        windowsVerbatimArguments: Isˉcommand,
    });
    const Result = await new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Output = [];
        const Errorˉoutput = [];
        let Outputˉbytes = 0;
        let Errorˉbytes = 0;
        let Exceeded = false;
        let Timedˉout = false;
        let Settled = false;
        let Cleanupˉfailure = null;
        let Terminationˉdiagnostic = null;
        let Terminationˉpromise = null;
        let Closeˉcode = null;
        let Closeˉsignal = null;
        let Closeˉreceived = false;
        let Settleˉtimer;
        const Started = Date.now();
        const Progressˉtimer = Options.Progressˉstep === undefined ? undefined :
            setInterval(() => {
                const Elapsedˉseconds = Math.floor((Date.now() - Started) / 1000);
                process.stdout.write(
                    `Progress: step=${Options.Progressˉstep} status=working ` +
                    `elapsed-seconds=${Elapsedˉseconds}\n`
                );
            }, PROGRESS_INTERVAL_MILLISECONDS);
        function Snapshot(Code, Signal, Forced) {
            return {
                Code,
                Signal,
                Output: Buffer.concat(Output).toString('utf8'),
                Errorˉoutput: Buffer.concat(Errorˉoutput).toString('utf8'),
                Exceeded,
                Timedˉout,
                Forced,
                Cleanupˉfailure,
            };
        }
        function Complete(Code, Signal, Forced) {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            if (Progressˉtimer !== undefined) clearInterval(Progressˉtimer);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Resolveˉresult(Snapshot(Code, Signal, Forced));
        }
        function Terminateˉandˉsettle() {
            if (Terminationˉpromise !== null) return;
            Settleˉtimer = setTimeout(() => {
                try {
                    Child.kill('SIGKILL');
                } catch (Errorˉvalue) {
                    Terminationˉdiagnostic = Terminationˉdiagnostic ??
                        `final direct child kill error: ${Errorˉvalue.message}`;
                }
                const Settleˉdiagnostic =
                    `process tree did not close within ${TERMINATION_SETTLE_MILLISECONDS} ms`;
                Cleanupˉfailure = Terminationˉdiagnostic === null
                    ? Settleˉdiagnostic
                    : `${Terminationˉdiagnostic}; ${Settleˉdiagnostic}`;
                Child.stdout.destroy();
                Child.stderr.destroy();
                Child.unref();
                Complete(null, null, true);
            }, TERMINATION_SETTLE_MILLISECONDS);
            Terminationˉpromise = (async () => {
                try {
                    Terminationˉdiagnostic = await Terminateˉprocessˉtree(Child);
                } catch (Errorˉvalue) {
                    Terminationˉdiagnostic =
                        `tree termination error: ${Errorˉvalue.message}`;
                    try { Child.kill('SIGKILL'); } catch {
                        // The bounded settle path records the diagnostic.
                    }
                }
                Cleanupˉfailure = Terminationˉdiagnostic;
                if (Closeˉreceived) Complete(Closeˉcode, Closeˉsignal, false);
            })();
        }
        const Timer = setTimeout(() => {
            Timedˉout = true;
            Terminateˉandˉsettle();
        }, Timeoutˉmilliseconds);
        Child.stdout.on('data', Chunk => {
            Outputˉbytes += Chunk.length;
            if (Outputˉbytes <= OUTPUT_LIMIT) Output.push(Chunk);
            else if (!Exceeded) {
                Exceeded = true;
                Terminateˉandˉsettle();
            }
        });
        Child.stderr.on('data', Chunk => {
            Errorˉbytes += Chunk.length;
            if (Errorˉbytes <= OUTPUT_LIMIT) Errorˉoutput.push(Chunk);
            else if (!Exceeded) {
                Exceeded = true;
                Terminateˉandˉsettle();
            }
        });
        Child.once('error', Errorˉvalue => {
            if (Timedˉout || Exceeded || Settled) return;
            Settled = true;
            clearTimeout(Timer);
            if (Progressˉtimer !== undefined) clearInterval(Progressˉtimer);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Rejectˉpromise(Errorˉvalue);
        });
        Child.once('close', (Code, Signal) => {
            Closeˉreceived = true;
            Closeˉcode = Code;
            Closeˉsignal = Signal;
            if (Terminationˉpromise === null) {
                Complete(Code, Signal, false);
                return;
            }
            void Terminationˉpromise.then(() => Complete(Code, Signal, false));
        });
    });
    const Label = Options.Label ?? basename(Command);
    if (Result.Cleanupˉfailure !== null) {
        Reject(`${Label} cleanup failed: ${Result.Cleanupˉfailure}.`);
    }
    if (Result.Timedˉout) throw Object.assign(new Error(
        `${Label} timed out after ${Timeoutˉmilliseconds} ms.`), { exitCode: 124 });
    if (Result.Exceeded) Reject(`${Label} exceeded the ${OUTPUT_LIMIT}-byte output limit.`);
    if (Options.Expectedˉexit !== undefined && Result.Code !== Options.Expectedˉexit) {
        Reject(
            `${Label} exited ${Result.Code}` +
            `${Result.Signal ? ` signal=${Result.Signal}` : ''}.` +
            `${Result.Output ? `\nstdout:\n${Result.Output}` : ''}` +
            `${Result.Errorˉoutput ? `\nstderr:\n${Result.Errorˉoutput}` : ''}`
        );
    }
    return Result;
}

function Nativeˉcommand(Host, Repository, Stem, Arguments) {
    const Native = join(Repository, 'Tools', 'Native');
    if (Host === 'windows') {
        return {
            Command: join(Native, `${Stem}.cmd`),
            Arguments,
        };
    }
    return {
        Command: join(Native, `${Stem}.sh`),
        Arguments,
    };
}

async function Runˉnative(Host, Repository, Step, Stem, Arguments) {
    process.stdout.write(`Progress: step=${Step} status=running\n`);
    const Invocation = Nativeˉcommand(Host, Repository, Stem, Arguments);
    return Runˉprocess(Invocation.Command, Invocation.Arguments, {
        Workingˉdirectory: Repository,
        Expectedˉexit: 0,
        Label: Stem,
        Progressˉstep: Step,
        Timeoutˉmilliseconds: CONSTRUCTION_TIMEOUT_MILLISECONDS,
    });
}

async function Buildˉcurrentˉtool(Host, Repository, Work, Name, Project, Application) {
    const Wvb = join(Work, Name + '.wvb');
    await Runˉnative(Host, Repository, Name + '-build', 'Build-Cached-Project-Wvb', [
        join(Repository, Project), Wvb,
    ]);
    const Package = await Runˉnative(Host, Repository, Name + '-package',
        'Package-Segmented-Compiler-Wvb', ['6', Wvb, Application, '--development-cache']);
    const Report = Package.Output.trim().split(/\r?\n/).at(-1);
    if (!/^segmented hosted WVB cache status=(Created|Hit) key=[0-9a-f]{64} host=(windows|linux)-x64 target=(windows|linux) profile=6$/.test(Report)) {
        Reject('The current-tool construction cache report differs.');
    }
    process.stdout.write('native SHA-256 construction tool=' + Name + ' ' + Report + '\n');
    for (const [Kind, Path] of [['wvb', Wvb], ['application', Application]]) {
        const Bytes = await Readˉbounded(Path, MAXIMUM_APPLICATION_BYTES, Name + ' ' + Kind);
        process.stdout.write('native SHA-256 product tool=' + Name + ' kind=' + Kind +
            ' bytes=' + Bytes.length + ' sha256=' + createHash('sha256').update(Bytes).digest('hex') + '\n');
    }
}

function Parseˉwvo(Bytes, Label) {
    if (Bytes.length < 24 || Bytes.subarray(0, 4).toString('ascii') !== 'WVO1') {
        Reject(`${Label} is not a Windvale object.`);
    }
    if (Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 0 ||
        Bytes[8] !== 1 || Bytes[9] !== 0 || Bytes.readUInt16LE(10) !== 0) {
        Reject(`${Label} envelope differs.`);
    }
    const Sectionˉcount = U32(Bytes, 12, `${Label} section count`);
    const Symbolˉcount = U32(Bytes, 16, `${Label} symbol count`);
    const Relocationˉcount = U32(Bytes, 20, `${Label} relocation count`);
    if (Sectionˉcount === 0 || Sectionˉcount > 16 || Symbolˉcount > 4096 ||
        Relocationˉcount > 65536) Reject(`${Label} counts exceed verifier bounds.`);
    let Cursor = 24;
    const Sections = [];
    for (let Index = 0; Index < Sectionˉcount; ++Index) {
        if (Cursor > Bytes.length || 20 > Bytes.length - Cursor) {
            Reject(`${Label} section header is truncated.`);
        }
        const Kind = Bytes[Cursor];
        const Flags = Bytes[Cursor + 1] | (Bytes[Cursor + 2] << 8) |
            (Bytes[Cursor + 3] << 16);
        const Align = U32(Bytes, Cursor + 4, `${Label} section alignment`);
        const Memory = U32(Bytes, Cursor + 8, `${Label} section memory length`);
        const Data = U32(Bytes, Cursor + 12, `${Label} section data length`);
        const Nameˉbytes = U32(Bytes, Cursor + 16, `${Label} section name length`);
        if (Nameˉbytes > Bytes.length - Cursor - 20) Reject(`${Label} section name is truncated.`);
        const Name = Bytes.subarray(Cursor + 20, Cursor + 20 + Nameˉbytes).toString('utf8');
        const Section = { Kind, Flags, Align, Memory, Dataˉbytes: Data, Name };
        Cursor += 20 + Nameˉbytes;
        if (Section.Dataˉbytes > Bytes.length - Cursor) Reject(`${Label} section data is truncated.`);
        Section.Dataˉoffset = Cursor;
        Section.Data = Bytes.subarray(Cursor, Cursor + Section.Dataˉbytes);
        Cursor += Section.Dataˉbytes;
        Sections.push(Section);
    }
    const Symbols = [];
    for (let Index = 0; Index < Symbolˉcount; ++Index) {
        if (Cursor > Bytes.length || 20 > Bytes.length - Cursor) Reject(`${Label} symbol is truncated.`);
        const Binding = Bytes[Cursor];
        const Kind = Bytes[Cursor + 1];
        const Flags = Bytes.readUInt16LE(Cursor + 2);
        const Section = U32(Bytes, Cursor + 4, `${Label} symbol section`);
        const Offset = U32(Bytes, Cursor + 8, `${Label} symbol offset`);
        const Size = U32(Bytes, Cursor + 12, `${Label} symbol size`);
        const Nameˉbytes = U32(Bytes, Cursor + 16, `${Label} symbol name length`);
        if (Nameˉbytes > Bytes.length - Cursor - 20) Reject(`${Label} symbol name is truncated.`);
        const Name = Bytes.subarray(Cursor + 20, Cursor + 20 + Nameˉbytes).toString('utf8');
        Symbols.push({ Binding, Kind, Flags, Section, Offset, Size, Name });
        Cursor += 20 + Nameˉbytes;
    }
    const Relocations = [];
    for (let Index = 0; Index < Relocationˉcount; ++Index) {
        if (Cursor > Bytes.length || 20 > Bytes.length - Cursor) Reject(`${Label} relocation is truncated.`);
        Relocations.push({
            Kind: Bytes[Cursor],
            Flags: Bytes[Cursor + 1] | (Bytes[Cursor + 2] << 8) |
                (Bytes[Cursor + 3] << 16),
            Section: U32(Bytes, Cursor + 4, `${Label} relocation section`),
            Offset: U32(Bytes, Cursor + 8, `${Label} relocation offset`),
            Symbol: U32(Bytes, Cursor + 12, `${Label} relocation symbol`),
            Addend: Bytes.readInt32LE(Cursor + 16),
        });
        Cursor += 20;
    }
    if (Cursor !== Bytes.length) Reject(`${Label} has trailing bytes.`);
    return { Sections, Symbols, Relocations };
}

function Findˉunique(Bytes, Pattern, Label) {
    const Positions = [];
    let Position = Bytes.indexOf(Pattern);
    while (Position !== -1) {
        Positions.push(Position);
        Position = Bytes.indexOf(Pattern, Position + 1);
    }
    if (Positions.length !== 1) Reject(`${Label} occurrence count is ${Positions.length}, expected 1.`);
    return Positions[0];
}

function Oracleˉhelper(Oracle) {
    const Text = Oracle.Sections.find(Section => Section.Name === '.text');
    const Readˉonly = Oracle.Sections.find(Section => Section.Name === '.rodata');
    if (!Text || !Readˉonly || Text.Data.length !== 1350 || Readˉonly.Data.length < 288) {
        Reject('The frozen SHA-256 WVO oracle shape differs.');
    }
    const Padding = Buffer.alloc((4 - (Text.Data.length % 4)) % 4);
    if (Padding.length !== 2) Reject('The frozen SHA-256 WVO oracle padding differs.');
    const Helper = Buffer.concat([Text.Data, Padding, Readˉonly.Data.subarray(0, 288)]);
    if (Helper.length !== HELPER_BYTES) Reject('The derived SHA-256 helper length differs.');
    for (const Relocation of Oracle.Relocations) {
        if (Relocation.Kind !== 2 || Relocation.Section !== 0 ||
            Relocation.Symbol >= Oracle.Symbols.length) {
            Reject('The frozen SHA-256 WVO oracle relocation shape differs.');
        }
        const Target = Oracle.Symbols[Relocation.Symbol];
        if (Target.Section !== 1 || Target.Offset >= 288 ||
            Relocation.Offset > Text.Data.length ||
            4 > Text.Data.length - Relocation.Offset) {
            Reject('The frozen SHA-256 WVO oracle relocation range differs.');
        }
        const Targetˉoffset = Text.Data.length + Padding.length + Target.Offset;
        Helper.writeInt32LE(
            Targetˉoffset - Relocation.Offset + Relocation.Addend,
            Relocation.Offset
        );
    }
    return Helper;
}

function Verifyˉhelperˉandˉcalls(Object, Oracle) {
    const Helpers = Object.Symbols.filter(Symbol => Symbol.Name === '$native_sha256_hex');
    if (Helpers.length !== 1) Reject(`Generated helper symbol count is ${Helpers.length}, expected 1.`);
    const Helper = Helpers[0];
    if (Helper.Binding !== 1 || Helper.Kind !== 1 || Helper.Size !== HELPER_BYTES ||
        Helper.Section >= Object.Sections.length) Reject('Generated helper symbol contract differs.');
    const Text = Object.Sections[Helper.Section];
    if (Helper.Offset > Text.Data.length || Helper.Size > Text.Data.length - Helper.Offset) {
        Reject('Generated helper range is invalid.');
    }
    Equal(
        Text.Data.subarray(Helper.Offset, Helper.Offset + Helper.Size),
        Oracleˉhelper(Oracle),
        'Generated helper bytes differ from the frozen WVO oracle.'
    );
    const Prefix = Buffer.from([
        65, 82, 65, 83, 86, 87, 69, 139, 140, 39, 60, 0, 0, 0,
        68, 137, 201, 129, 193, 64, 0, 0, 0, 15, 130,
    ]);
    let Wrappers = 0;
    for (const Symbol of Object.Symbols) {
        if (Symbol.Kind !== 1 || Symbol.Section !== Helper.Section || Symbol === Helper ||
            Symbol.Offset > Text.Data.length || Symbol.Size > Text.Data.length - Symbol.Offset) continue;
        const Code = Text.Data.subarray(Symbol.Offset, Symbol.Offset + Symbol.Size);
        let Position = Code.indexOf(Prefix);
        while (Position !== -1) {
            if (Position + 152 > Code.length || Code[Position + 112] !== 0xe8) {
                Reject('Generated SHA-256 wrapper call field differs.');
            }
            const Target = Symbol.Offset + Position + 117 +
                Code.readInt32LE(Position + 113);
            if (Target !== Helper.Offset) {
                Reject('Generated SHA-256 wrapper does not target the private helper.');
            }
            ++Wrappers;
            Position = Code.indexOf(Prefix, Position + 152);
        }
    }
    if (Wrappers !== 4) {
        Reject(`Generated SHA-256 wrapper count is ${Wrappers}, expected exactly 4.`);
    }
}

function Verifyˉshaˉinstructionˉcontract(Object) {
    const Text = Object.Sections.find(Section => Section.Name === '.text');
    if (!Text) Reject('SHA smoke object has no .text section.');
    const Prefix = Buffer.from([
        65, 82, 65, 83, 86, 87, 69, 139, 140, 39, 60, 0, 0, 0,
        68, 137, 201, 129, 193, 64, 0, 0, 0, 15, 130,
    ]);
    const Start = Findˉunique(Text.Data, Prefix, 'SHA-256 intrinsic instruction');
    const Instruction = Text.Data.subarray(Start, Start + 152);
    if (Instruction.length !== 152) Reject('SHA-256 intrinsic instruction is truncated.');
    const Capacityˉbranch = Buffer.from([
        69, 139, 132, 39, 56, 0, 0, 0, 68, 57, 193, 15, 135,
    ]);
    const Commit = Buffer.from([65, 137, 140, 39, 60, 0, 0, 0]);
    const Detailˉtwo = Buffer.from([
        184, 2, 0, 0, 0, 65, 137, 132, 39, 64, 0, 0, 0,
    ]);
    const Branchˉposition = Instruction.indexOf(Capacityˉbranch);
    const Commitˉposition = Instruction.indexOf(Commit);
    const Failureˉposition = Instruction.indexOf(Detailˉtwo);
    if (Branchˉposition < 0 || Commitˉposition <= Branchˉposition || Failureˉposition <= Commitˉposition) {
        Reject('SHA-256 64-byte allocation atomicity/detail-2 instruction contract differs.');
    }
}

function Contextˉpattern() {
    const Pattern = Buffer.alloc(CONTEXT_BYTES);
    Pattern.writeUInt32LE(7, 0);
    Pattern.writeUInt32LE(CONTEXT_BYTES, 4);
    Pattern.writeBigUInt64LE(1000000n, 8);
    Pattern.writeBigUInt64LE(1024n, 16);
    Pattern.writeUInt32LE(2097152, 40);
    Pattern.writeUInt32LE(16777216, 56);
    return Pattern;
}

async function Patchˉcontext(Source, Target, Offset, Patch) {
    const Bytes = await Readˉbounded(
        Source, MAXIMUM_APPLICATION_BYTES, `${basename(Source)} application`
    );
    const Context = Findˉunique(Bytes, Contextˉpattern(), `${basename(Source)} execution context`);
    if (U32(Bytes, Context + 60, 'execution-context current arena') !== 0) {
        Reject('Packaged execution-context arena cursor is not zero.');
    }
    await copyFile(Source, Target);
    const Handle = await open(Target, 'r+');
    try {
        const Written = await Handle.write(Patch, 0, Patch.length, Context + Offset);
        if (Written.bytesWritten !== Patch.length) Reject('Execution-context capacity patch was partial.');
    } finally {
        await Handle.close();
    }
    const Patched = await Readˉbounded(
        Target, MAXIMUM_APPLICATION_BYTES, `${basename(Target)} patched application`
    );
    const Expected = Buffer.from(Bytes);
    Patch.copy(Expected, Context + Offset);
    Equal(Patched, Expected, 'Execution-context clone changed bytes outside its selected field.');
}

async function Patchˉarenaˉcapacity(Source, Target, Capacity) {
    const Patch = Buffer.alloc(4);
    Patch.writeUInt32LE(Capacity);
    await Patchˉcontext(Source, Target, 56, Patch);
}

async function Main() {
    const Arguments = process.argv.slice(4);
    const Streamingˉonly = Arguments[0] === '--streaming';
    if (Streamingˉonly) Arguments.shift();
    let Maximumˉseconds = Streamingˉonly ? 60 : 600;
    if (Arguments.length === 2 && Arguments[0] === '--maximum-seconds' &&
        /^[1-9][0-9]{0,3}$/.test(Arguments[1]) && Number(Arguments[1]) <= 3600) {
        Maximumˉseconds = Number(Arguments[1]);
        Arguments.length = 0;
    }
    if (process.argv.length < 4 || Arguments.length !== 0 ||
        !['windows', 'linux'].includes(process.argv[2])) {
        process.stderr.write('Usage: node Test-Native-Sha256.mjs <windows|linux> <repository-root> [--streaming] [--maximum-seconds <1-3600>]\n');
        process.exitCode = 64;
        return;
    }
    Runˉdeadline = Date.now() + Maximumˉseconds * 1000;
    const Host = process.argv[2];
    const Repository = resolve(process.argv[3]);
    const Temporaryˉprefix = join(tmpdir(), 'windvale-native-sha256.');
    const Work = await mkdtemp(Temporaryˉprefix);
    let Tests = 0;
    const Pass = Message => {
        ++Tests;
        process.stdout.write(`PASS  native SHA-256 ${Message}\n`);
    };
    try {
        const Extension = Host === 'windows' ? 'exe' : 'elf';
        if (Streamingˉonly) {
            const Wvb = join(Work, 'Streaming.wvb');
            const Application = join(Work, 'Streaming.' + Extension);
            await Runˉnative(Host, Repository, 'streaming-build', 'Build-Cached-Project-Wvb', [
                join(Repository, 'Projects/Tests/Windvale-Native-Test-Sha256-Streaming.wvproj'), Wvb,
            ]);
            await Runˉnative(Host, Repository, 'streaming-package', 'Package-Segmented-Compiler-Wvb', [
                '1', Wvb, Application, '--development-cache',
            ]);
            const Execution = await Runˉprocess(Application, [], {
                Workingˉdirectory: Work, Expectedˉexit: 42,
                Label: 'streaming SHA-256 cases', Progressˉstep: 'streaming-execution',
            });
            if (Execution.Output !== '' || Execution.Errorˉoutput !== '') {
                Reject('The streaming SHA-256 self-test emitted unexpected output.');
            }
            const Wvbˉbytes = await Readˉbounded(Wvb, MAXIMUM_WVO_BYTES, 'streaming WVB');
            process.stdout.write('native SHA-256 streaming development status=Passed cases=20 kats=12 malformed=7 boundary=1 qualification=false ' +
                'wvb-bytes=' + Wvbˉbytes.length + ' wvb-sha256=' +
                createHash('sha256').update(Wvbˉbytes).digest('hex') + '\n');
            return;
        }
        const Lowerer = join(Work, `Wvb-To-Wvo.${Extension}`);
        await Buildˉcurrentˉtool(Host, Repository, Work, 'lowerer',
            'Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj', Lowerer);
        Pass('current lowerer construction');

        const Returnˉinput = join(Repository, 'Artifacts', 'Native-Wvb-To-Wvo-Candidate', 'Return-42.wvb');
        const Returnˉexpected = join(Repository, 'Artifacts', 'Native-Wvb-To-Wvo-Candidate', 'Return-42.wvo');
        const Returnˉactual = join(Work, 'Return-42.wvo');
        await Runˉprocess(Lowerer, [Returnˉinput, Returnˉactual], {
            Workingˉdirectory: Repository,
            Expectedˉexit: 0,
            Label: 'current SHA-capable lowerer',
            Progressˉstep: 'sha-free-lowering',
        });
        Equal(
            await Readˉbounded(Returnˉactual, MAXIMUM_WVO_BYTES, 'lowered Return-42 WVO'),
            await Readˉbounded(Returnˉexpected, MAXIMUM_WVO_BYTES, 'retained Return-42 WVO'),
            'SHA-free Return-42 WVO bytes changed.');
        Pass('SHA-free WVO identity');

        const Katˉwvb = join(Work, 'Sha256-Kat.wvb');
        const Katˉwvo = join(Work, 'Sha256-Kat.wvo');
        await Runˉnative(Host, Repository, 'kat-build', 'Build-Cached-Project-Wvb', [
            join(Repository, 'Projects/Tests/Windvale-Native-Test-Sha256-Native-Kat.wvproj'),
            Katˉwvb,
        ]);
        await Runˉprocess(Lowerer, [Katˉwvb, Katˉwvo], {
            Workingˉdirectory: Repository,
            Expectedˉexit: 0,
            Label: 'SHA-256 KAT lowering',
            Progressˉstep: 'kat-lowering',
        });
        await Runˉnative(Host, Repository, 'kat-object-check', 'Check-Wvo', [Katˉwvo]);
        const Katˉbytes = await Readˉbounded(
            Katˉwvo, MAXIMUM_WVO_BYTES, 'SHA-256 KAT WVO'
        );
        const Katˉobject = Parseˉwvo(Katˉbytes, 'SHA-256 KAT WVO');
        const Oracle = Parseˉwvo(
            await Readˉbounded(
                join(Repository, 'Linker', 'Reference', 'Consumers',
                    'X64-Wvb-Publication-Sha256.wvo'),
                MAXIMUM_WVO_BYTES,
                'frozen SHA-256 WVO oracle'
            ),
            'frozen SHA-256 WVO oracle'
        );
        Verifyˉhelperˉandˉcalls(Katˉobject, Oracle);
        Pass('one 1640-byte oracle-exact local helper and call target');

        const Katˉimage = join(Work, 'Sha256-Kat.bin');
        const Katˉapplication = join(Work, `Sha256-Kat.${Extension}`);
        await Runˉnative(Host, Repository, 'kat-link', 'Link-Wvo', [
            '0', 'Main', Katˉimage, Katˉwvo,
        ]);
        await Runˉnative(Host, Repository, 'kat-package', 'Package-Console', [
            `${Host}-x64-console-v1`, Katˉimage, '0', Katˉapplication,
        ]);
        // The console smoke default is one million instructions; the 1 MiB
        // streaming workload needs a larger, explicit execution allowance.
        // Keep the packaged arena and every other byte unchanged.
        const Katˉbounded = join(Work, 'Sha256-Kat-Bounded.' + Extension);
        const Katˉbudget = Buffer.alloc(8);
        Katˉbudget.writeBigUInt64LE(4294967296n);
        await Patchˉcontext(Katˉapplication, Katˉbounded, 8, Katˉbudget);
        await Runˉprocess(Katˉbounded, [], {
            Workingˉdirectory: Work,
            Expectedˉexit: 42,
            Label: 'native SHA-256 exact KAT application',
            Progressˉstep: 'kat-execution',
        });
        Pass('empty and abc exact native KATs plus 20 streaming cases');

        const Stager = join(Work, `Wvb-To-Wvo-Stager.${Extension}`);
        await Buildˉcurrentˉtool(Host, Repository, Work, 'publication-stager',
            'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj', Stager);
        const Stagedˉprefix = join(Work, 'Sha256-Staged');
        const Stagedˉmanifest = join(Work, 'Sha256-Staged.wvop');
        await Runˉprocess(Stager, [Katˉwvb, Stagedˉprefix, Stagedˉmanifest], {
            Workingˉdirectory: Repository,
            Expectedˉexit: 0,
            Label: 'source-built SHA-256 publication stager',
            Progressˉstep: 'publication-stager-run',
        });
        const Manifest = await Readˉbounded(
            Stagedˉmanifest, MAXIMUM_MANIFEST_BYTES, 'SHA-256 staging manifest'
        );
        if (Manifest.length < 24 || Manifest.subarray(0, 4).toString('ascii') !== 'WVOP' ||
            Manifest.readUInt16LE(4) !== 1 || Manifest.readUInt16LE(6) !== 0 ||
            U32(Manifest, 8, 'staging manifest length') !== Manifest.length ||
            U32(Manifest, 20, 'staging chunk limit') !== 4194304) {
            Reject('Source-built SHA-256 publication manifest differs.');
        }
        const Objectˉbytes = U32(Manifest, 12, 'staged object length');
        const Chunks = U32(Manifest, 16, 'staged chunk count');
        if (Objectˉbytes !== Katˉbytes.length || Objectˉbytes > MAXIMUM_WVO_BYTES ||
            Chunks === 0 || Chunks > 518 || Manifest.length !== 24 + Chunks * 12) {
            Reject('Source-built SHA-256 publication chunk count differs.');
        }
        let Position = 0;
        let Helperˉcoalesced = false;
        const Helperˉabsolute = Katˉobject.Sections[0].Dataˉoffset +
            Katˉobject.Symbols.find(Symbol => Symbol.Name === '$native_sha256_hex').Offset;
        for (let Index = 0; Index < Chunks; ++Index) {
            const Entry = 24 + Index * 12;
            const Entryˉindex = U32(Manifest, Entry, 'staged chunk index');
            const Entryˉposition = U32(Manifest, Entry + 4, 'staged chunk position');
            const Entryˉbytes = U32(Manifest, Entry + 8, 'staged chunk length');
            if (Entryˉindex !== Index || Entryˉposition !== Position || Entryˉbytes === 0 ||
                Entryˉbytes > MAXIMUM_STAGING_CHUNK_BYTES ||
                Entryˉbytes > Objectˉbytes - Position) {
                Reject('Source-built SHA-256 publication chunk sequence differs.');
            }
            const Chunk = await Readˉbounded(
                `${Stagedˉprefix}.chunk-${Index}`,
                MAXIMUM_STAGING_CHUNK_BYTES,
                `staged SHA-256 chunk ${Index}`
            );
            if (Chunk.length !== Entryˉbytes) Reject('Staged SHA-256 chunk length differs.');
            const End = Position + Entryˉbytes;
            Equal(
                Chunk,
                Katˉbytes.subarray(Position, End),
                `Source-built staged SHA-256 chunk ${Index} differs from the direct WVO.`
            );
            if (Position < Helperˉabsolute && End >= Helperˉabsolute + HELPER_BYTES) {
                Helperˉcoalesced = true;
            }
            Position = End;
        }
        if (Position !== Objectˉbytes || !Helperˉcoalesced) {
            Reject('Source-built publication did not coalesce the final function and SHA helper.');
        }
        Pass('publication steps plus staged coalesced-helper corruption rejection');

        const Smokeˉwvb = join(Work, 'Sha256-Smoke.wvb');
        const Smokeˉwvo = join(Work, 'Sha256-Smoke.wvo');
        const Smokeˉimage = join(Work, 'Sha256-Smoke.bin');
        const Smokeˉapplication = join(Work, `Sha256-Smoke.${Extension}`);
        await Runˉnative(Host, Repository, 'arena-smoke-build', 'Build-Cached-Project-Wvb', [
            join(Repository, 'Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Sha256.wvproj'),
            Smokeˉwvb,
        ]);
        await Runˉprocess(Lowerer, [Smokeˉwvb, Smokeˉwvo], {
            Workingˉdirectory: Repository,
            Expectedˉexit: 0,
            Label: 'SHA-256 arena-smoke lowering',
            Progressˉstep: 'arena-smoke-lowering',
        });
        await Runˉnative(
            Host, Repository, 'arena-smoke-object-check', 'Check-Wvo', [Smokeˉwvo]
        );
        const Smokeˉobject = Parseˉwvo(
            await Readˉbounded(Smokeˉwvo, MAXIMUM_WVO_BYTES, 'SHA-256 arena-smoke WVO'),
            'SHA-256 arena-smoke WVO'
        );
        Verifyˉshaˉinstructionˉcontract(Smokeˉobject);
        Pass('atomic allocation and detail-2 machine contract');
        await Runˉnative(Host, Repository, 'arena-smoke-link', 'Link-Wvo', [
            '0', 'Main', Smokeˉimage, Smokeˉwvo,
        ]);
        await Runˉnative(Host, Repository, 'arena-smoke-package', 'Package-Console', [
            `${Host}-x64-console-v1`, Smokeˉimage, '0', Smokeˉapplication,
        ]);
        const Capacity64 = join(Work, `Sha256-Capacity-64.${Extension}`);
        const Capacity63 = join(Work, `Sha256-Capacity-63.${Extension}`);
        await Patchˉarenaˉcapacity(Smokeˉapplication, Capacity64, 64);
        await Patchˉarenaˉcapacity(Smokeˉapplication, Capacity63, 63);
        await Runˉprocess(Capacity64, [], {
            Workingˉdirectory: Work,
            Expectedˉexit: 42,
            Label: '64-byte SHA-256 arena application',
            Progressˉstep: 'arena-64-execution',
        });
        Pass('64-byte arena success');
        await Runˉprocess(Capacity63, [], {
            Workingˉdirectory: Work,
            Expectedˉexit: 1,
            Label: '63-byte SHA-256 arena application',
            Progressˉstep: 'arena-63-execution',
        });
        Pass('63-byte arena atomic failure detail=2');

        if (Tests !== 8) Reject(`Native SHA-256 case count is ${Tests}, expected 8.`);
        process.stdout.write(
            'native SHA-256 lowering status=Passed cases=8 kats=2 arena=64/63 ' +
            'helper-bytes=1640 sha-free=Identical staged-corruption=Rejected streaming-cases=20\n'
        );
    } finally {
        const Resolved = resolve(Work);
        if (!Resolved.startsWith(resolve(tmpdir()) + '\\') &&
            !Resolved.startsWith(resolve(tmpdir()) + '/')) {
            Reject(`Refusing to remove unexpected temporary path: ${Resolved}`);
        }
        await rm(Resolved, { recursive: true, force: true });
        if (Date.now() >= Runˉdeadline) {
            process.exitCode = 124;
            process.stderr.write('Native SHA-256 total budget expired during cleanup.\n');
        }
    }
}

Main().catch(Errorˉvalue => {
    process.stderr.write(`Native SHA-256 verification failed: ${Errorˉvalue.message}\n`);
    process.exitCode = Errorˉvalue.exitCode ?? 1;
});
