import { spawn, spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import { existsSync } from 'node:fs';
import {
    lstat,
    mkdir,
    mkdtemp,
    readFile,
    realpath,
    rm,
    writeFile,
} from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_TOOL_BYTES = 134_217_728;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_MODULE_BYTES = 1_048_576;
const MAXIMUM_WIR_BYTES = 4_194_304;
const MAXIMUM_WVB_BYTES = 16_777_216;
const ANALYSIS_TIMEOUT_MILLISECONDS = 120_000;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SYSTEM_HEADER =
    'profile system; platform linux, windows, windvale; authority application; ';

if (process.argv.length !== 5 && process.argv.length !== 6) Usage();

const Analyzer = resolve(process.argv[2]);
const Emitter = resolve(process.argv[3]);
const Verifier = resolve(process.argv[4]);
const Runner = process.argv.length === 6 ? resolve(process.argv[5]) : undefined;
for (const [Tool, Label] of [
    [Analyzer, 'Analyzer'],
    [Emitter, 'emitter'],
    [Verifier, 'verifier'],
    ...(Runner === undefined ? [] : [[Runner, 'runner']]),
]) {
    const Status = await lstat(Tool);
    if (!Status.isFile() || Status.size <= 0 ||
        Status.size > MAXIMUM_TOOL_BYTES || await realpath(Tool) !== Tool) {
        Reject(
            `The unsafe-scratch ${Label} must be an ordinary canonical file.`,
        );
    }
}

const Resultˉmodule = await Readˉeditionˉoneˉbody(
    join(REPOSITORY_ROOT, 'Libraries', 'Foundation', 'Values', 'Result.wv'),
    'Foundation result module',
);
const Memoryˉmodule = await Readˉeditionˉoneˉbody(
    join(REPOSITORY_ROOT, 'Libraries', 'Foundation', 'Memory', 'Memory.wv'),
    'Foundation memory module',
);
const Unsafeˉmodule = await Readˉeditionˉoneˉbody(
    join(REPOSITORY_ROOT, 'Libraries', 'Foundation', 'Unsafe', 'Unsafe.wv'),
    'Foundation unsafe module',
);

const Imports =
    'import Foundationˉmemory as Memory; ' +
    'import Foundationˉresult as Result; ' +
    'import Foundationˉunsafe as Unsafe; ';
const Abi =
    'enum Hostˉabi: u8 { Windows = 1u8; } ' +
    'enum Otherˉabi: u8 { Witness = 1u8; } ';
const Resultˉtype =
    'Result.Result<Unsafe.Foreignˉscratch<Hostˉabi>, ' +
    'Unsafe.Foreignˉmemoryˉfailure>';
const Validˉcall =
    'Unsafe.Constructˉscratch::<Hostˉabi>(' +
    'Budget: Budget, Length: 64u64, Alignment: 16u64)';

function Application(Parameters, Returnˉtype, Call, Effects =
    ' effects(memory.allocate)') {
    return 'module Languageˉoneˉunsafeˉscratchˉwir; ' + SYSTEM_HEADER +
        Imports + Abi + 'export fn Construct(' + Parameters + ') -> ' +
        Returnˉtype + Effects + ' { return ' + Call + '; } ' +
        'export fn Main() -> i32 { return 42; }';
}

const Cases = [
    {
        Name: 'valid-canonical-scratch',
        Expected: 'valid',
        Source: Application(
            'Budget: Memory.Memoryˉbudget', Resultˉtype, Validˉcall,
        ),
    },
    {
        Name: 'missing-explicit-abi',
        Expected: 'Genericˉresolution',
        Source: Application(
            'Budget: Memory.Memoryˉbudget', Resultˉtype,
            'Unsafe.Constructˉscratch(' +
                'Budget: Budget, Length: 64u64, Alignment: 16u64)',
        ),
    },
    {
        Name: 'scalar-abi',
        Expected: 'Genericˉresolution',
        Source: Application(
            'Budget: Memory.Memoryˉbudget', Resultˉtype,
            'Unsafe.Constructˉscratch::<u8>(' +
                'Budget: Budget, Length: 64u64, Alignment: 16u64)',
        ),
    },
    {
        Name: 'wrong-result-abi',
        Expected: 'Genericˉresolution',
        Source: Application(
            'Budget: Memory.Memoryˉbudget',
            'Result.Result<Unsafe.Foreignˉscratch<Otherˉabi>, ' +
                'Unsafe.Foreignˉmemoryˉfailure>',
            Validˉcall,
        ),
    },
    {
        Name: 'wrong-result-failure',
        Expected: 'Genericˉresolution',
        Source: Application(
            'Budget: Memory.Memoryˉbudget',
            'Result.Result<Unsafe.Foreignˉscratch<Hostˉabi>, ' +
                'Memory.Allocationˉfailure>',
            Validˉcall,
        ),
    },
    {
        Name: 'wrong-budget',
        Expected: 'Invalidˉargument',
        Source: Application('Budget: u64', Resultˉtype, Validˉcall),
    },
    {
        Name: 'u32-length',
        Expected: 'Invalidˉargument',
        Source: Application(
            'Budget: Memory.Memoryˉbudget', Resultˉtype,
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
                'Budget: Budget, Length: 64u32, Alignment: 16u64)',
        ),
    },
    {
        Name: 'u32-alignment',
        Expected: 'Invalidˉargument',
        Source: Application(
            'Budget: Memory.Memoryˉbudget', Resultˉtype,
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
                'Budget: Budget, Length: 64u64, Alignment: 16u32)',
        ),
    },
    {
        Name: 'missing-allocation-effect',
        Expected: 'valid',
        Source: Application(
            'Budget: Memory.Memoryˉbudget', Resultˉtype, Validˉcall, '',
        ),
    },
];

const Runtimeˉapplications = [
    {
        Name: 'success',
        Source: Runtimeˉapplication(
            '',
            'let Outcome: ' + Resultˉtype + ' = ' +
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
            'Budget: Budget, Length: 64u64, Alignment: 8u64); ' +
            'return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 42 } ' +
            'case Result.Result.Failure { Error: _ } { 1 } };',
        ),
    },
    {
        Name: 'invalid-length',
        Source: Runtimeˉapplication(
            'fn Summarize(Error: Unsafe.Foreignˉmemoryˉfailure) -> i32 { ' +
            'return match Error { ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Invalidˉlength { ' +
            'Observed: Observed, Maximum: Maximum } { ' +
            'if Observed == 0u64 && Maximum == 64u64 { 42 } else { 2 } } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Invalidˉalignment { ' +
            'Observed: _ } { 3 } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Allocation { Error: _ } { 4 } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Unsupportedˉabi { 5 } }; } ',
            'let Outcome: ' + Resultˉtype + ' = ' +
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
            'Budget: Budget, Length: 0u64, Alignment: 8u64); ' +
            'return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 1 } ' +
            'case Result.Result.Failure { Error: Failure } { ' +
            'Summarize(Failure) } };',
        ),
    },
    {
        Name: 'invalid-alignment',
        Source: Runtimeˉapplication(
            'fn Summarize(Error: Unsafe.Foreignˉmemoryˉfailure) -> i32 { ' +
            'return match Error { ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Invalidˉlength { ' +
            'Observed: _, Maximum: _ } { 2 } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Invalidˉalignment { ' +
            'Observed: Observed } { ' +
            'if Observed == 16u64 { 42 } else { 3 } } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Allocation { Error: _ } { 4 } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Unsupportedˉabi { 5 } }; } ',
            'let Outcome: ' + Resultˉtype + ' = ' +
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
            'Budget: Budget, Length: 8u64, Alignment: 16u64); ' +
            'return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 1 } ' +
            'case Result.Result.Failure { Error: Failure } { ' +
            'Summarize(Failure) } };',
        ),
    },
];

function Runtimeˉapplication(Helpers, Body) {
    return 'module Languageˉoneˉunsafeˉscratchˉruntime; ' + SYSTEM_HEADER +
        Imports + Abi + Helpers +
        'export fn Main(Budget: Memory.Memoryˉbudget) -> i32 ' +
        'effects(memory.allocate) { ' + Body + ' }';
}

const Work = await mkdtemp(join(tmpdir(), 'windvale-unsafe-scratch-wir-'));
var Valid = 0;
var Rejected = 0;
var Malformed = 0;
var Wvbˉmalformed = 0;
var Wvbˉverified = 0;
var Wvbˉbytes = 0;
var Wvbˉsha256 = '';
try {
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const Case = Cases[Index];
        process.stdout.write(
            'native language 1 unsafe scratch WVIR ' +
            `item=${Index + 1}/${Cases.length} case=${Case.Name} ` +
            'status=Started\n',
        );
        const Caseˉdirectory = join(
            Work, `${String(Index).padStart(2, '0')}-${Case.Name}`,
        );
        await mkdir(Caseˉdirectory);
        const Input = join(Caseˉdirectory, 'Source.wvss');
        const Sourceˉoutput = join(Caseˉdirectory, 'Output.wvss');
        const Manifest = join(Caseˉdirectory, 'Manifest.wvca');
        const Bindings = join(Caseˉdirectory, 'Bindings.wvlb');
        const Wir = join(Caseˉdirectory, 'Wir.wvir');
        await writeFile(Input, Sourceˉset([
            Case.Source, Memoryˉmodule, Resultˉmodule, Unsafeˉmodule,
        ]), { flag: 'wx' });
        const Analysis = await Runˉanalyzer([
            '--internal-source-set', Input,
            Sourceˉoutput, Manifest, Bindings, Wir,
        ]);
        if (Case.Expected === 'valid') {
            if (Analysis.Code !== 0 || Analysis.Exceeded) {
                Reject(
                    `Unsafe-scratch case ${Case.Name} failed with status ` +
                    `${Analysis.Code}.\n${Analysis.Diagnostic}`,
                );
            }
            const Wirˉbytes = await readFile(Wir);
            const Layout = Inspectˉvalidˉwir(Wirˉbytes);
            if (Case.Name === 'valid-canonical-scratch') {
                const Boundary = await Verifyˉemitterˉboundary(
                    Sourceˉoutput, Manifest, Bindings, Wir, Wirˉbytes,
                    Layout, Caseˉdirectory,
                );
                Malformed += Boundary.Wir;
                Wvbˉmalformed += Boundary.Wvb;
                Wvbˉverified += Boundary.Verifier;
            }
            Valid += 1;
        } else {
            if (Analysis.Code !== 1 || Analysis.Exceeded ||
                !Analysis.Diagnostic.includes(
                    `wir-status=${Case.Expected}`,
                )) {
                Reject(
                    `Unsafe-scratch rejection ${Case.Name} differed.\n` +
                    Analysis.Diagnostic,
                );
            }
            Rejected += 1;
        }
    }
    if (Runner !== undefined) {
        await Verifyˉruntime();
    }
    process.stdout.write(
        'native language 1 unsafe scratch WVIR status=Passed ' +
        `cases=${Cases.length} valid=${Valid} rejected=${Rejected} ` +
        `malformed=${Malformed} wvb-malformed=${Wvbˉmalformed} ` +
        `operation=186 opcode=220 wvb-minor=33 wvb-bytes=${Wvbˉbytes} ` +
        `wvb-sha256=${Wvbˉsha256} effect-check=emitter ` +
        `runtime=${Runner === undefined ? 'Notˉrequested' : 'Verified'} ` +
        `compiler-verifier-cases=${Wvbˉverified}\n`,
    );
} finally {
    await Removeˉwork(Work);
}

async function Verifyˉruntime() {
    const Directory = join(Work, 'runtime');
    await mkdir(Directory);
    for (let Index = 0; Index < Runtimeˉapplications.length; Index += 1) {
        const Case = Runtimeˉapplications[Index];
        process.stdout.write(
            'native language 1 unsafe scratch runtime ' +
            `item=${Index + 1}/${Runtimeˉapplications.length} ` +
            `case=${Case.Name} status=Started\n`,
        );
        const Caseˉdirectory = join(Directory, Case.Name);
        await mkdir(Caseˉdirectory);
        const Input = join(Caseˉdirectory, 'Source.wvss');
        const Sourceˉoutput = join(Caseˉdirectory, 'Output.wvss');
        const Manifest = join(Caseˉdirectory, 'Manifest.wvca');
        const Bindings = join(Caseˉdirectory, 'Bindings.wvlb');
        const Wir = join(Caseˉdirectory, 'Wir.wvir');
        const Wvb = join(Caseˉdirectory, 'Runtime.wvb');
        await writeFile(Input, Sourceˉset([
            Case.Source, Memoryˉmodule, Resultˉmodule, Unsafeˉmodule,
        ]), { flag: 'wx' });
        const Analysis = await Runˉanalyzer([
            '--internal-source-set', Input,
            Sourceˉoutput, Manifest, Bindings, Wir,
        ]);
        if (Analysis.Code !== 0 || Analysis.Exceeded) {
            Reject(
                `Unsafe-scratch runtime ${Case.Name} analysis failed: ` +
                `status=${Analysis.Code}.\n${Analysis.Diagnostic}`,
            );
        }
        const Emission = await Runˉemitter(
            Sourceˉoutput, Manifest, Bindings, Wir, Wvb,
        );
        if (Emission.Code !== 0 || Emission.Exceeded || !Exists(Wvb)) {
            Reject(
                `Unsafe-scratch runtime ${Case.Name} emission failed: ` +
                `status=${Emission.Code}.\n${Emission.Diagnostic}`,
            );
        }
        await Requireˉwvbˉverification(Wvb, true, `runtime-${Case.Name}`);
        const Execution = spawnSync(Runner, [Wvb], {
            cwd: Work,
            encoding: 'utf8',
            windowsHide: true,
            maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
            timeout: ANALYSIS_TIMEOUT_MILLISECONDS,
        });
        if (Execution.error !== undefined || Execution.status !== 0 ||
            Execution.stdout.replaceAll('\r\n', '\n') !== 'Result: 42\n' ||
            Execution.stderr.length !== 0) {
            Reject(
                `Unsafe-scratch runtime ${Case.Name} execution differed: ` +
                `status=${Execution.status} ` +
                `error=${Execution.error?.message ?? 'none'}\n` +
                `stdout=${Execution.stdout}\nstderr=${Execution.stderr}`,
            );
        }
        if (Case.Name === 'success') {
            const Candidate = await readFile(Wvb);
            const Layout = Inspectˉunsafeˉscratchˉwvb(Candidate);
            Candidate[Layout.Operation] = 209;
            const Missingˉoperation = join(
                Caseˉdirectory, 'Missing-Scratch-Operation.wvb',
            );
            await writeFile(Missingˉoperation, Candidate, { flag: 'wx' });
            const Rejected = spawnSync(Runner, [Missingˉoperation], {
                cwd: Work,
                encoding: 'utf8',
                windowsHide: true,
                maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
                timeout: ANALYSIS_TIMEOUT_MILLISECONDS,
            });
            if (Rejected.error !== undefined || Rejected.status !== 1 ||
                Rejected.stdout.length !== 0 ||
                !Rejected.stderr.replaceAll('\r\n', '\n').endsWith(
                    ' phase=execution\n',
                )) {
                Reject(
                    'Unsafe-scratch missing-operation runtime rejection ' +
                    `differed: status=${Rejected.status} ` +
                    `error=${Rejected.error?.message ?? 'none'}\n` +
                    `stdout=${Rejected.stdout}\nstderr=${Rejected.stderr}`,
                );
            }
        }
    }
    process.stdout.write(
        'native language 1 unsafe scratch runtime status=Passed ' +
        `cases=${Runtimeˉapplications.length} malformed=1 result=42 ` +
        'allocation=zeroed teardown=bounded\n',
    );
}

function Inspectˉvalidˉwir(Input) {
    if (Input.length < 56 || Input.length > MAXIMUM_WIR_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVIR' ||
        Input.readUInt16LE(4) !== 1 ||
        (Input.readUInt16LE(6) !== 23 && Input.readUInt16LE(6) !== 24) ||
        Input.readUInt32LE(12) !== 48 || Input.readUInt32LE(20) !== 28 ||
        Input.readUInt32LE(28) !== 28 || Input.readUInt32LE(36) !== 4 ||
        Input.readUInt32LE(44) !== 4) {
        Reject('The valid unsafe-scratch analysis did not publish exact WVIR.');
    }
    const Headerˉbytes = Input.readUInt16LE(6) === 24 ? 64 : 56;
    const Functions = Input.readUInt32LE(8);
    const Blocks = Input.readUInt32LE(16);
    const Operations = Input.readUInt32LE(24);
    const Temporaries = Input.readUInt32LE(32);
    const Operands = Input.readUInt32LE(40);
    const Operationsˉoffset = Headerˉbytes + Functions * 48 + Blocks * 28;
    const Temporariesˉoffset = Operationsˉoffset + Operations * 28;
    const Operandsˉoffset = Temporariesˉoffset + Temporaries * 4;
    if (Operandsˉoffset + Operands * 4 > Input.length) {
        Reject(
            'The unsafe-scratch WVIR directory offsets exceed the file: ' +
            `minor=${Input.readUInt16LE(6)} header=${Headerˉbytes} ` +
            `functions=${Functions} blocks=${Blocks} operations=${Operations} ` +
            `temporaries=${Temporaries} operands=${Operands} bytes=${Input.length}.`,
        );
    }
    const Matches = [];
    for (let Index = 0; Index < Operations; Index += 1) {
        const Entry = Operationsˉoffset + Index * 28;
        if (Input.readUInt16LE(Entry + 4) === 186) {
            Matches.push({ Entry, Index });
        }
    }
    if (Matches.length !== 1) {
        Reject('The valid unsafe-scratch WVIR must contain operation 186 once.');
    }
    const Operation = Matches[0].Entry;
    const Shape = Input.readUInt32LE(Operation + 8);
    const Temporary = Input.readUInt32LE(Operation + 12);
    const Firstˉoperand = Input.readUInt32LE(Operation + 16);
    const Target = Input.readUInt32LE(Operation + 20);
    const Abiˉshape = Input.readUInt32LE(Operation + 24);
    if (Input.readUInt16LE(Operation + 6) !== 2 || Shape < 0x80000000 ||
        Temporary >= Temporaries || Firstˉoperand > Operands - 2 ||
        Target !== 0 || Abiˉshape < 131_072 || Abiˉshape >= 196_608 ||
        Input.readUInt32LE(Temporariesˉoffset + Temporary * 4) !== Shape) {
        Reject('The unsafe-scratch operation header evidence differs.');
    }
    const Operandˉtemporaries = [];
    for (let Index = 0; Index < 2; Index += 1) {
        const Operandˉtemporary = Input.readUInt32LE(
            Operandsˉoffset + (Firstˉoperand + Index) * 4,
        );
        if (Operandˉtemporary >= Temporary ||
            Input.readUInt32LE(
                Temporariesˉoffset + Operandˉtemporary * 4,
            ) !== 8) {
            Reject('The unsafe-scratch length/alignment evidence differs.');
        }
        Operandˉtemporaries.push(Operandˉtemporary);
    }
    return {
        Operation,
        Operationˉindex: Matches[0].Index,
        Resultˉshape: Temporariesˉoffset + Temporary * 4,
        Lengthˉshape: Temporariesˉoffset + Operandˉtemporaries[0] * 4,
        Alignmentˉshape: Temporariesˉoffset + Operandˉtemporaries[1] * 4,
    };
}

async function Verifyˉemitterˉboundary(
    Source, Manifest, Bindings, Wir, Valid, Layout, Directory,
) {
    const Publishedˉoutput = join(Directory, 'unsafe-scratch.wvb');
    const Published = await Runˉemitter(
        Source, Manifest, Bindings, Wir, Publishedˉoutput,
    );
    if (Published.Code !== 0 || Published.Exceeded ||
        !/^source emission status=Published mode=optimized functions=[0-9]+ code-bytes=[0-9]+ module-bytes=[0-9]+\n$/u.test(
            Published.Diagnostic,
        ) || !Exists(Publishedˉoutput)) {
        Reject(
            'The valid operation-186 WVB publication differed: ' +
            `status=${Published.Code} diagnostic=${JSON.stringify(
                Published.Diagnostic,
            )}.`,
        );
    }
    const Publishedˉbytes = await readFile(Publishedˉoutput);
    Wvbˉbytes = Publishedˉbytes.length;
    Wvbˉsha256 = createHash('sha256').update(Publishedˉbytes).digest('hex');
    const Wvbˉlayout = Inspectˉunsafeˉscratchˉwvb(Publishedˉbytes);
    var Verifierˉcases = 0;
    await Requireˉwvbˉverification(
        Publishedˉoutput, true, 'valid-canonical-scratch',
    );
    Verifierˉcases += 1;
    const Wvbˉcases = [
        {
            Name: 'old-wvb-minor',
            Mutate: Candidate => Candidate.writeUInt16LE(32, 6),
        },
        {
            Name: 'unknown-wvb-opcode',
            Mutate: Candidate => { Candidate[Wvbˉlayout.Operation] = 221; },
        },
        {
            Name: 'missing-wvb-scratch-operation',
            Mutate: Candidate => { Candidate[Wvbˉlayout.Operation] = 209; },
        },
        {
            Name: 'invalid-wvb-budget-slot',
            Mutate: Candidate => Candidate.writeUInt32LE(
                0xffff_ffff, Wvbˉlayout.Operation + 1,
            ),
        },
        {
            Name: 'invalid-wvb-result-type',
            Mutate: Candidate => Candidate.writeUInt32LE(
                Wvbˉlayout.Typeˉcount, Wvbˉlayout.Operation + 5,
            ),
        },
        {
            Name: 'invalid-wvb-abi-type',
            Mutate: Candidate => Candidate.writeUInt32LE(
                Wvbˉlayout.Typeˉcount, Wvbˉlayout.Operation + 9,
            ),
        },
        {
            Name: 'non-enum-wvb-abi-type',
            Mutate: Candidate => Candidate.writeUInt32LE(
                Wvbˉlayout.Resultˉtype, Wvbˉlayout.Operation + 9,
            ),
        },
    ];
    for (let Index = 0; Index < Wvbˉcases.length; Index += 1) {
        const Case = Wvbˉcases[Index];
        process.stdout.write(
            'native language 1 unsafe scratch WVB ' +
            `malformed-item=${Index + 1}/${Wvbˉcases.length} ` +
            `case=${Case.Name} status=Started\n`,
        );
        const Candidate = Buffer.from(Publishedˉbytes);
        Case.Mutate(Candidate);
        Requireˉwvbˉcontractˉrejection(Candidate, Case.Name);
        const Candidateˉpath = join(
            Directory, `wvb-${Case.Name}.wvb`,
        );
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        await Requireˉwvbˉverification(Candidateˉpath, false, Case.Name);
        Verifierˉcases += 1;
    }
    const Semanticˉcases = [
        {
            Name: 'wrong-shape-wvb-budget-slot',
            Mutate: Candidate => Candidate.writeUInt32LE(
                1, Wvbˉlayout.Operation + 1,
            ),
        },
        {
            Name: 'noncanonical-wvb-result-name',
            Mutate: Candidate => {
                Candidate[Wvbˉlayout.Resultˉname] ^=
                    Candidate[Wvbˉlayout.Resultˉname] === 0x58 ? 1 : 0x58;
            },
        },
        {
            Name: 'noncanonical-wvb-allocation-field',
            Mutate: Candidate => {
                Candidate[Wvbˉlayout.Allocationˉfieldˉname] ^= 1;
            },
        },
    ];
    for (let Index = 0; Index < Semanticˉcases.length; Index += 1) {
        const Case = Semanticˉcases[Index];
        process.stdout.write(
            'native language 1 unsafe scratch WVB ' +
            `semantic-item=${Index + 1}/${Semanticˉcases.length} ` +
            `case=${Case.Name} status=Started\n`,
        );
        const Candidate = Buffer.from(Publishedˉbytes);
        Case.Mutate(Candidate);
        const Candidateˉpath = join(
            Directory, `wvb-${Case.Name}.wvb`,
        );
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        await Requireˉwvbˉverification(Candidateˉpath, false, Case.Name);
        Verifierˉcases += 1;
    }
    const Invalidˉanalysis =
        /^source emission status=Invalidˉanalysis analysis-status=Invalidˉwir wvb-status=Sourceˉwir function=0 operation=0 source-line=0\n$/u;
    const Cases = [
        {
            Name: 'old-minor',
            Mutate: Candidate => Candidate.writeUInt16LE(22, 6),
        },
        {
            Name: 'unknown-operation',
            Mutate: Candidate => Candidate.writeUInt16LE(
                65_535, Layout.Operation + 4,
            ),
        },
        {
            Name: 'primitive-result',
            Mutate: Candidate => Candidate.writeUInt32LE(
                8, Layout.Operation + 8,
            ),
        },
        {
            Name: 'missing-alignment-operand',
            Mutate: Candidate => Candidate.writeUInt16LE(
                1, Layout.Operation + 6,
            ),
        },
        {
            Name: 'invalid-budget-slot',
            Mutate: Candidate => Candidate.writeUInt32LE(
                0xffff_ffff, Layout.Operation + 20,
            ),
        },
        {
            Name: 'invalid-abi-shape',
            Mutate: Candidate => Candidate.writeUInt32LE(
                0, Layout.Operation + 24,
            ),
        },
        {
            Name: 'u32-length-shape',
            Mutate: Candidate => Candidate.writeUInt32LE(3, Layout.Lengthˉshape),
        },
        {
            Name: 'u32-alignment-shape',
            Mutate: Candidate => Candidate.writeUInt32LE(
                3, Layout.Alignmentˉshape,
            ),
        },
        {
            Name: 'result-temporary-shape-mismatch',
            Mutate: Candidate => Candidate.writeUInt32LE(8, Layout.Resultˉshape),
        },
    ];
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const Case = Cases[Index];
        process.stdout.write(
            'native language 1 unsafe scratch WVIR ' +
            `malformed-item=${Index + 1}/${Cases.length} ` +
            `case=${Case.Name} status=Started\n`,
        );
        const Candidate = Buffer.from(Valid);
        Case.Mutate(Candidate);
        const Candidateˉpath = join(Directory, `${Case.Name}.wvir`);
        const Output = join(Directory, `${Case.Name}.wvb`);
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉemitterˉrejection(
            await Runˉemitter(
                Source, Manifest, Bindings, Candidateˉpath, Output,
            ),
            Invalidˉanalysis, Case.Name, Output,
        );
    }
    return {
        Wir: Cases.length,
        Wvb: Wvbˉcases.length,
        Verifier: Verifierˉcases,
    };
}

function Inspectˉunsafeˉscratchˉwvb(Input) {
    if (Input.length < 12 || Input.length > MAXIMUM_WVB_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 33 ||
        Input.readUInt32LE(8) !== 7) {
        Reject('The unsafe-scratch WVB header differs.');
    }
    const Sections = new Map();
    var Cursor = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Cursor > Input.length - 8 || Input[Cursor] !== Kind ||
            Input[Cursor + 1] !== 0 || Input.readUInt16LE(Cursor + 2) !== 0) {
            Reject('The unsafe-scratch WVB section envelope differs.');
        }
        const Length = Input.readUInt32LE(Cursor + 4);
        const Start = Cursor + 8;
        if (Start > Input.length || Length > Input.length - Start) {
            Reject('The unsafe-scratch WVB section exceeds the file.');
        }
        Sections.set(Kind, { Start, End: Start + Length });
        Cursor = Start + Length;
    }
    if (Cursor !== Input.length) {
        Reject('The unsafe-scratch WVB has trailing bytes.');
    }
    const Types = Sections.get(7);
    const Typeˉcount = Input.readUInt32LE(Types.Start);
    if (Typeˉcount === 0 || Typeˉcount > 65_536) {
        Reject('The unsafe-scratch WVB type count is invalid.');
    }
    const Typeˉkinds = [];
    const Typeˉstarts = [];
    Cursor = Types.Start + 4;
    for (let Index = 0; Index < Typeˉcount; Index += 1) {
        if (Cursor >= Types.End) {
            Reject('The unsafe-scratch WVB type directory is truncated.');
        }
        Typeˉkinds.push(Input[Cursor]);
        Typeˉstarts.push(Cursor);
        Cursor = Nextˉwvbˉtype(Input, Cursor, Types.End);
    }
    if (Cursor !== Types.End) {
        Reject('The unsafe-scratch WVB type directory has trailing bytes.');
    }
    const Code = Sections.get(5);
    const Matches = [];
    for (Cursor = Code.Start; Cursor <= Code.End - 13; Cursor += 1) {
        if (Input[Cursor] !== 220 || Input.readUInt32LE(Cursor + 1) !== 0) {
            continue;
        }
        const Resultˉtype = Input.readUInt32LE(Cursor + 5);
        const Abiˉtype = Input.readUInt32LE(Cursor + 9);
        if (Resultˉtype < Typeˉcount && Abiˉtype < Typeˉcount &&
            Typeˉkinds[Resultˉtype] === 3 &&
            (Typeˉkinds[Abiˉtype] === 2 || Typeˉkinds[Abiˉtype] === 7)) {
            Matches.push({ Operation: Cursor, Resultˉtype, Abiˉtype });
        }
    }
    if (Matches.length !== 1) {
        Reject('The unsafe-scratch WVB must contain one exact opcode 220.');
    }
    const Resultˉdescriptor = Typeˉstarts[Matches[0].Resultˉtype];
    const Resultˉnameˉlength = Input.readUInt32LE(Resultˉdescriptor + 1);
    if (Resultˉnameˉlength === 0) {
        Reject('The unsafe-scratch WVB result type name is empty.');
    }
    const Allocationˉfield = Buffer.from('Reason', 'utf8');
    const Allocationˉfieldˉname = Input.indexOf(
        Allocationˉfield, Types.Start,
    );
    const Duplicateˉallocationˉfieldˉname = Input.indexOf(
        Allocationˉfield,
        Allocationˉfieldˉname + Allocationˉfield.length,
    );
    if (Allocationˉfieldˉname < Types.Start ||
        Allocationˉfieldˉname + Allocationˉfield.length > Types.End ||
        (Duplicateˉallocationˉfieldˉname >= 0 &&
            Duplicateˉallocationˉfieldˉname < Types.End)) {
        Reject('The unsafe-scratch WVB allocation field identity differs.');
    }
    return {
        Allocationˉfieldˉname,
        Operation: Matches[0].Operation,
        Resultˉtype: Matches[0].Resultˉtype,
        Abiˉtype: Matches[0].Abiˉtype,
        Resultˉname: Resultˉdescriptor + 5,
        Typeˉcount,
    };
}

function Nextˉwvbˉtype(Input, Start, End) {
    const Kind = Input[Start];
    var Cursor = Start + 1;
    if (Kind === 8) {
        Cursor = Checkedˉadvance(Cursor, 1, End);
        Cursor = Checkedˉshape(Input, Cursor, End);
        const Parameters = Checkedˉu32(Input, Cursor, End);
        Cursor += 4;
        if (Parameters > 64) {
            Reject('The unsafe-scratch WVB callable arity is oversized.');
        }
        for (let Index = 0; Index < Parameters; Index += 1) {
            Cursor = Checkedˉshape(Input, Cursor, End);
        }
        return Checkedˉadvance(Cursor, 10 + Parameters, End);
    }
    if (Kind < 1 || Kind > 7) {
        Reject('The unsafe-scratch WVB type kind is unknown.');
    }
    Cursor = Checkedˉstring(Input, Cursor, End);
    if (Kind === 4) {
        Cursor = Checkedˉshape(Input, Cursor, End);
        return Checkedˉadvance(Cursor, 4, End);
    }
    if (Kind === 5 || Kind === 6) {
        return Checkedˉshape(Input, Cursor, End);
    }
    if (Kind === 7) {
        Cursor = Checkedˉadvance(Cursor, 1, End);
    }
    const Items = Checkedˉu32(Input, Cursor, End);
    Cursor += 4;
    if (Items > 256) {
        Reject('The unsafe-scratch WVB type item count is oversized.');
    }
    for (let Item = 0; Item < Items; Item += 1) {
        Cursor = Checkedˉstring(Input, Cursor, End);
        if (Kind === 1) {
            Cursor = Checkedˉshape(Input, Cursor, End);
        } else if (Kind === 2) {
            Cursor = Checkedˉadvance(Cursor, 4, End);
        } else if (Kind === 7) {
            Cursor = Checkedˉadvance(Cursor, 1, End);
        } else {
            const Encoding = Input[Cursor];
            Cursor = Checkedˉadvance(Cursor, 1, End);
            if (Encoding === 1) {
                Cursor = Checkedˉstring(Input, Cursor, End);
                Cursor = Checkedˉshape(Input, Cursor, End);
            } else if (Encoding === 2) {
                const Fields = Checkedˉu32(Input, Cursor, End);
                Cursor += 4;
                if (Fields < 2 || Fields > 64) {
                    Reject('The unsafe-scratch WVB variant field count is invalid.');
                }
                for (let Field = 0; Field < Fields; Field += 1) {
                    Cursor = Checkedˉstring(Input, Cursor, End);
                    Cursor = Checkedˉshape(Input, Cursor, End);
                }
            } else if (Encoding !== 0) {
                Reject('The unsafe-scratch WVB variant encoding is invalid.');
            }
        }
    }
    return Cursor;
}

function Checkedˉshape(Input, Cursor, End) {
    const Kind = Input[Checkedˉadvance(Cursor, 1, End) - 1];
    if ([7, 8, 11, 22, 23, 24, 26, 27, 28, 29, 30, 35].includes(Kind)) {
        return Checkedˉadvance(Cursor + 1, 4, End);
    }
    return Cursor + 1;
}

function Checkedˉstring(Input, Cursor, End) {
    const Length = Checkedˉu32(Input, Cursor, End);
    return Checkedˉadvance(Cursor + 4, Length, End);
}

function Checkedˉu32(Input, Cursor, End) {
    Checkedˉadvance(Cursor, 4, End);
    return Input.readUInt32LE(Cursor);
}

function Checkedˉadvance(Cursor, Length, End) {
    if (!Number.isSafeInteger(Cursor) || !Number.isSafeInteger(Length) ||
        Cursor < 0 || Length < 0 || Cursor > End || Length > End - Cursor) {
        Reject('The unsafe-scratch WVB directory is truncated.');
    }
    return Cursor + Length;
}

function Requireˉwvbˉcontractˉrejection(Input, Label) {
    try {
        Inspectˉunsafeˉscratchˉwvb(Input);
    } catch {
        return;
    }
    Reject(`The malformed unsafe-scratch WVB was accepted: ${Label}.`);
}

function Requireˉemitterˉrejection(Result, Expected, Label, Output) {
    if (Result.Code !== 1 || Result.Exceeded ||
        !Expected.test(Result.Diagnostic) || Exists(Output)) {
        Reject(
            `The unsafe-scratch emitter rejection ${Label} differed: ` +
            `status=${Result.Code} diagnostic=${JSON.stringify(
                Result.Diagnostic,
            )}.`,
        );
    }
}

async function Requireˉwvbˉverification(Module, Valid, Label) {
    const Result = await Runˉtool(Verifier, [Module]);
    const Diagnostic = Result.Diagnostic.replaceAll('\r\n', '\n');
    if (Result.Exceeded) {
        Reject(`The unsafe-scratch verifier ${Label} exceeded its bounds.`);
    }
    if (Valid) {
        if (Result.Code !== 0 || Diagnostic !==
            'wvb status=Valid profile=compiler-aligned\n') {
            Reject(
                `The unsafe-scratch verifier acceptance ${Label} differed: ` +
                `status=${Result.Code} diagnostic=${JSON.stringify(
                    Diagnostic,
                )}.`,
            );
        }
        return;
    }
    if (Result.Code === 0 || !Diagnostic.includes('wvb status=Invalid')) {
        Reject(
            `The unsafe-scratch verifier rejection ${Label} differed: ` +
            `status=${Result.Code} diagnostic=${JSON.stringify(Diagnostic)}.`,
        );
    }
}

function Exists(Path) {
    return existsSync(Path);
}

async function Readˉeditionˉoneˉbody(Path, Label) {
    const Canonical = await realpath(Path);
    const Status = await lstat(Canonical);
    if (!Status.isFile() || Status.size <= 0 ||
        Status.size > MAXIMUM_MODULE_BYTES) {
        Reject(`The ${Label} is not a bounded ordinary file.`);
    }
    const Source = (await readFile(Canonical)).toString('utf8');
    const Descriptor = '#!wv/1 en@1\n';
    if (!Source.startsWith(Descriptor)) {
        Reject(`The ${Label} does not have the canonical edition-1 descriptor.`);
    }
    return Source.slice(Descriptor.length);
}

function Sourceˉset(Modules) {
    if (Modules.length < 1 || Modules.length > 8) {
        Reject('The unsafe-scratch source set module count is invalid.');
    }
    const Sources = Modules.map(Source => Buffer.from(Source, 'utf8'));
    const Headerˉbytes = 16 + Sources.length * 20;
    const Header = Buffer.alloc(Headerˉbytes);
    Header.write('WVSS', 0, 'ascii');
    Header.writeUInt16LE(2, 4);
    Header.writeUInt16LE(0, 6);
    Header.writeUInt32LE(Sources.length, 8);
    Header.writeUInt32LE(Sources.length * 20, 12);
    var Offset = Headerˉbytes;
    for (let Index = 0; Index < Sources.length; Index += 1) {
        const Entry = 16 + Index * 20;
        Header.writeUInt32LE(Offset, Entry);
        Header.writeUInt32LE(Sources[Index].length, Entry + 4);
        Header.writeUInt32LE(1, Entry + 8);
        Header.writeUInt32LE(1, Entry + 12);
        Header.writeUInt32LE(1, Entry + 16);
        Offset += Sources[Index].length;
    }
    return Buffer.concat([Header, ...Sources]);
}

function Runˉanalyzer(Arguments) {
    return Runˉtool(Analyzer, Arguments);
}

function Runˉemitter(Source, Manifest, Bindings, Wir, Output) {
    return Runˉtool(Emitter, [Source, Manifest, Bindings, Wir, Output]);
}

function Runˉtool(Tool, Arguments) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Child = spawn(Tool, Arguments, {
            cwd: Work,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
        });
        const Output = [];
        var Outputˉbytes = 0;
        var Exceeded = false;
        const Capture = Chunk => {
            Outputˉbytes += Chunk.length;
            if (Outputˉbytes <= MAXIMUM_DIAGNOSTIC_BYTES) {
                Output.push(Chunk);
            } else {
                Exceeded = true;
                Child.kill();
            }
        };
        Child.stdout.on('data', Capture);
        Child.stderr.on('data', Capture);
        Child.once('error', Rejectˉpromise);
        const Timeout = setTimeout(() => {
            Exceeded = true;
            Child.kill();
        }, ANALYSIS_TIMEOUT_MILLISECONDS);
        Child.once('close', Code => {
            clearTimeout(Timeout);
            Resolveˉresult({
                Code,
                Diagnostic: Buffer.concat(Output).toString('utf8'),
                Exceeded,
            });
        });
    });
}

async function Removeˉwork(Path) {
    const Temporaryˉroot = await realpath(resolve(tmpdir()));
    const Parent = await realpath(dirname(Path));
    if (Parent !== Temporaryˉroot ||
        !basename(Path).startsWith('windvale-unsafe-scratch-wir-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Path}`);
    }
    await rm(Path, { force: false, maxRetries: 2, recursive: true });
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/Test-Language-1.0-Unsafe-Scratch-Wir.mjs ' +
        '<analyzer> <emitter> <compiler-verifier> [runner]\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
