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

if (process.argv.length < 5 || process.argv.length > 7) Usage();

const Analyzer = resolve(process.argv[2]);
const Emitter = resolve(process.argv[3]);
const Verifier = resolve(process.argv[4]);
const Runner = process.argv.length >= 6 ? resolve(process.argv[5]) : undefined;
const Lowerer = process.argv.length === 7 ? resolve(process.argv[6]) : undefined;
const Nativeˉextension = process.platform === 'win32' ? 'cmd' : 'sh';
const Nativeˉchecker = resolve(
    SCRIPT_DIRECTORY, `Check-Wvo.${Nativeˉextension}`,
);
const Nativeˉlinker = resolve(
    SCRIPT_DIRECTORY, `Link-Wvo.${Nativeˉextension}`,
);
const Nativeˉpackager = resolve(
    SCRIPT_DIRECTORY, `Package-Console.${Nativeˉextension}`,
);
for (const [Tool, Label] of [
    [Analyzer, 'Analyzer'],
    [Emitter, 'emitter'],
    [Verifier, 'verifier'],
    ...(Runner === undefined ? [] : [[Runner, 'runner']]),
    ...(Lowerer === undefined ? [] : [[Lowerer, 'native lowerer']]),
    ...(Lowerer === undefined ? [] : [
        [Nativeˉchecker, 'native object checker'],
        [Nativeˉlinker, 'native linker'],
        [Nativeˉpackager, 'native console packager'],
    ]),
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
    {
        Name: 'reused-call-budget',
        Expected: 'emitter-invalid-wir',
        Source: 'module Languageˉoneˉunsafeˉscratchˉwir; ' + SYSTEM_HEADER +
            Imports +
            'fn Consume(Budget: Memory.Memoryˉbudget) -> i32 { return 1; } ' +
            'export fn Main(Budget: Memory.Memoryˉbudget) -> i32 { ' +
            'let First: i32 = Consume(Budget); ' +
            'return First + Consume(Budget); }',
    },
    {
        Name: 'borrow-call-budget',
        Expected: 'valid',
        Source: 'module Languageˉoneˉunsafeˉscratchˉwir; ' + SYSTEM_HEADER +
            Imports +
            'fn Observe(Budget: borrow Memory.Memoryˉbudget) -> i32 { ' +
            'return 42; } ' +
            'export fn Run(Budget: Memory.Memoryˉbudget) -> i32 { ' +
            'return Observe(borrow Budget); } ' +
            'export fn Main() -> i32 { return 42; }',
    },
];

const Runtimeˉapplications = [
    {
        Name: 'success',
        Nativeˉexecution: true,
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
        Nativeˉexecution: true,
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
        Nativeˉexecution: true,
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
    {
        Name: 'call-transfer-success',
        Nativeˉexecution: true,
        Source: Runtimeˉapplication(
            'fn Allocate(Scratchˉbudget: Memory.Memoryˉbudget) -> i32 ' +
            'effects(memory.allocate) { ' +
            'let Outcome: ' + Resultˉtype + ' = ' +
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
            'Budget: Scratchˉbudget, Length: 64u64, Alignment: 8u64); ' +
            'return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 42 } ' +
            'case Result.Result.Failure { Error: _ } { 1 } }; } ',
            'return Allocate(Budget);',
        ),
    },
    {
        Name: 'split-call-transfer-success',
        Nativeˉexecution: true,
        Source: Runtimeˉapplication(
            'fn Allocate(Scratchˉbudget: Memory.Memoryˉbudget) -> i32 ' +
            'effects(memory.allocate) { ' +
            'let Outcome: ' + Resultˉtype + ' = ' +
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
            'Budget: Scratchˉbudget, Length: 32u64, Alignment: 8u64); ' +
            'return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 42 } ' +
            'case Result.Result.Failure { Error: _ } { 1 } }; } ',
            'var Parent: Memory.Memoryˉbudget = Budget; ' +
            'let Splitˉresult: Result.Result<' +
            'Memory.Memoryˉbudget, Memory.Allocationˉfailure> = ' +
            'Memory.Split(borrow mut Parent, 64u64, 1u32); ' +
            'return match Splitˉresult { ' +
            'case Result.Result.Valid { Value: Child } { Allocate(Child) } ' +
            'case Result.Result.Failure { Error: _ } { 2 } };',
        ),
    },
    {
        Name: 'split-budget-refusal-parent-preserved',
        Nativeˉexecution: true,
        Source: Runtimeˉapplication(
            'fn Allocateˉafterˉrefusal(' +
            'Scratchˉbudget: Memory.Memoryˉbudget, ' +
            'Allocationˉerror: Memory.Allocationˉfailure) -> i32 ' +
            'effects(memory.allocate) { ' +
            'if Allocationˉerror.Reason != ' +
            'Memory.Allocationˉreason.Budgetˉexhausted || ' +
            'Allocationˉerror.Requestedˉbytes != 131072u64 { ' +
            'return 3; } ' +
            'let Outcome: ' + Resultˉtype + ' = ' +
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
            'Budget: Scratchˉbudget, Length: 64u64, Alignment: 8u64); ' +
            'return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 42 } ' +
            'case Result.Result.Failure { Error: _ } { 4 } }; } ',
            'var Parent: Memory.Memoryˉbudget = Budget; ' +
            'let Splitˉresult: Result.Result<' +
            'Memory.Memoryˉbudget, Memory.Allocationˉfailure> = ' +
            'Memory.Split(borrow mut Parent, 131072u64, 1u32); ' +
            'return match Splitˉresult { ' +
            'case Result.Result.Valid { Value: _ } { 2 } ' +
            'case Result.Result.Failure { Error: Failure } { ' +
            'Allocateˉafterˉrefusal(Parent, Failure) } };',
        ),
    },
    {
        Name: 'split-call-budget-refusal',
        Nativeˉexecution: true,
        Source: Runtimeˉapplication(
            'fn Summarizeˉallocation(' +
            'Allocationˉerror: Memory.Allocationˉfailure) -> i32 { ' +
            'if Allocationˉerror.Reason == ' +
            'Memory.Allocationˉreason.Budgetˉexhausted && ' +
            'Allocationˉerror.Requestedˉbytes == 64u64 && ' +
            'Allocationˉerror.Availableˉbytes == 32u64 { ' +
            'return 42; } return 5; } ' +
            'fn Summarizeˉforeign(' +
            'Foreignˉerror: Unsafe.Foreignˉmemoryˉfailure) -> i32 { ' +
            'return match Foreignˉerror { ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Invalidˉlength { ' +
            'Observed: _, Maximum: _ } { 2 } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Invalidˉalignment { ' +
            'Observed: _ } { 3 } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Allocation { ' +
            'Error: Allocationˉerror } { ' +
            'Summarizeˉallocation(Allocationˉerror) } ' +
            'case Unsafe.Foreignˉmemoryˉfailure.Unsupportedˉabi { 4 } }; } ' +
            'fn Allocateˉbeyondˉchild(' +
            'Scratchˉbudget: Memory.Memoryˉbudget) -> i32 ' +
            'effects(memory.allocate) { ' +
            'let Outcome: ' + Resultˉtype + ' = ' +
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
            'Budget: Scratchˉbudget, Length: 64u64, Alignment: 8u64); ' +
            'return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 1 } ' +
            'case Result.Result.Failure { Error: Foreignˉerror } { ' +
            'Summarizeˉforeign(Foreignˉerror) } }; } ',
            'var Parent: Memory.Memoryˉbudget = Budget; ' +
            'let Splitˉresult: Result.Result<' +
            'Memory.Memoryˉbudget, Memory.Allocationˉfailure> = ' +
            'Memory.Split(borrow mut Parent, 32u64, 1u32); ' +
            'return match Splitˉresult { ' +
            'case Result.Result.Valid { Value: Child } { ' +
            'Allocateˉbeyondˉchild(Child) } ' +
            'case Result.Result.Failure { Error: _ } { 6 } };',
        ),
    },
    {
        Name: 'borrow-call-parent-preserved',
        Nativeˉexecution: true,
        Source: Runtimeˉapplication(
            'fn Observe(Budget: borrow Memory.Memoryˉbudget) -> i32 { ' +
            'return 7; } ' +
            'fn Allocate(Scratchˉbudget: Memory.Memoryˉbudget) -> i32 ' +
            'effects(memory.allocate) { ' +
            'let Outcome: ' + Resultˉtype + ' = ' +
            'Unsafe.Constructˉscratch::<Hostˉabi>(' +
            'Budget: Scratchˉbudget, Length: 64u64, Alignment: 8u64); ' +
            'return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 42 } ' +
            'case Result.Result.Failure { Error: _ } { 1 } }; } ',
            'let Observation: i32 = Observe(borrow Budget); ' +
            'if Observation != 7 { return 2; } ' +
            'return Allocate(Budget);',
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
var Borrowedˉbudgetˉrejections = 0;
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
            if (Case.Name === 'valid-canonical-scratch') {
                const Layout = Inspectˉvalidˉwir(Wirˉbytes);
                const Boundary = await Verifyˉemitterˉboundary(
                    Sourceˉoutput, Manifest, Bindings, Wir, Wirˉbytes,
                    Layout, Caseˉdirectory,
                );
                Malformed += Boundary.Wir;
                Wvbˉmalformed += Boundary.Wvb;
                Wvbˉverified += Boundary.Verifier;
            }
            Valid += 1;
        } else if (Case.Expected === 'emitter-invalid-wir' ||
            Case.Expected === 'emitter-unsupported-shape') {
            if (Analysis.Code !== 0 || Analysis.Exceeded) {
                Reject(
                    `Unsafe-scratch ownership case ${Case.Name} failed ` +
                    `analysis with status ${Analysis.Code}.\n` +
                    Analysis.Diagnostic,
                );
            }
            const Wvb = join(Caseˉdirectory, 'Rejected.wvb');
            const Emission = await Runˉemitter(
                Sourceˉoutput, Manifest, Bindings, Wir, Wvb,
            );
            const Expectedˉdiagnostic = Case.Expected ===
                'emitter-invalid-wir'
                ? 'analysis-status=Invalidˉwir'
                : 'analysis-status=Valid wvb-status=Unsupportedˉshape';
            if (Emission.Code !== 1 || Emission.Exceeded || Exists(Wvb) ||
                !Emission.Diagnostic.includes(Expectedˉdiagnostic)) {
                Reject(
                    `Unsafe-scratch ownership rejection ${Case.Name} ` +
                    `differed.\n${Emission.Diagnostic}`,
                );
            }
            Rejected += 1;
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
    if (process.env.WINDVALE_KEEP_TEST_WORK === '1') {
        process.stdout.write(`native language 1 unsafe scratch work=${Work}\n`);
    } else {
        await Removeˉwork(Work);
    }
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
        if (Case.Name === 'borrow-call-parent-preserved') {
            Borrowedˉbudgetˉrejections =
                await Verifyˉborrowedˉbudgetˉwvbˉrejections(
                    Wvb, Caseˉdirectory,
                );
        }
        if (Lowerer !== undefined && Case.Nativeˉaot !== false) {
            const Wvo = join(Caseˉdirectory, 'Runtime.wvo');
            await Requireˉnativeˉlowering(Wvb, Wvo, Case.Name, true);
            if (Case.Nativeˉexecution === true) {
                await Requireˉnativeˉexecution(Wvo, Caseˉdirectory, Case.Name);
            }
            if (Case.Name === 'split-call-transfer-success') {
                await Verifyˉnativeˉsplitˉrejections(Wvb, Caseˉdirectory);
            }
        }
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
            if (Lowerer !== undefined) {
                await Verifyˉnativeˉownershipˉrejections(
                    Candidate, Layout, Caseˉdirectory,
                );
            }
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
        `cases=${Runtimeˉapplications.length} ` +
        `malformed=${1 + Borrowedˉbudgetˉrejections} result=42 ` +
        `native-aot=${Lowerer === undefined ? 'Notˉrequested' :
            Runtimeˉapplications.filter(Case => Case.Nativeˉaot !== false).length +
            '/' + Runtimeˉapplications.length} ` +
        `native-execution=${Lowerer === undefined ? 'Notˉrequested' :
            Runtimeˉapplications.filter(Case => Case.Nativeˉexecution === true).length +
            '/' + Runtimeˉapplications.length} ` +
        `native-split-rejections=${Lowerer === undefined ? 'Notˉrequested' : 3} ` +
        `borrowed-budget-rejections=${Borrowedˉbudgetˉrejections} ` +
        'allocation=zeroed teardown=bounded\n',
    );
}

async function Requireˉnativeˉlowering(
    Wvb, Wvo, Label, Valid, Expectedˉdetail = 0, Expectedˉfunction = 0,
) {
    const Result = await Runˉtool(Lowerer, [Wvb, Wvo]);
    const Diagnostic = Result.Diagnostic.replaceAll('\r\n', '\n');
    if (Result.Exceeded) {
        Reject(`The unsafe-scratch native lowerer ${Label} exceeded its bounds.`);
    }
    if (Valid) {
        if (Result.Code !== 0 || !Exists(Wvo) ||
            !/^native x64 status=Valid abi=22 code-bytes=[0-9]+ object-bytes=[0-9]+\n$/u.test(
                Diagnostic,
            )) {
            Reject(
                `The unsafe-scratch native lowering ${Label} differed: ` +
                `status=${Result.Code} diagnostic=${JSON.stringify(Diagnostic)}.`,
            );
        }
        return;
    }
    if (Result.Code !== 1 || Exists(Wvo) ||
        !Diagnostic.includes(
            `status=Unsupportedˉcode function=${Expectedˉfunction} ` +
            `detail=${Expectedˉdetail}`,
        )) {
        Reject(
            `The unsafe-scratch native rejection ${Label} differed: ` +
            `status=${Result.Code} diagnostic=${JSON.stringify(Diagnostic)}.`,
        );
    }
}

async function Requireˉnativeˉrejection(Wvb, Wvo, Label) {
    const Result = await Runˉtool(Lowerer, [Wvb, Wvo]);
    const Diagnostic = Result.Diagnostic.replaceAll('\r\n', '\n');
    if (Result.Exceeded) {
        Reject(`The unsafe-scratch native lowerer ${Label} exceeded its bounds.`);
    }
    if (Result.Code !== 1 || Exists(Wvo) ||
        !/^native x64 status=(?:Invalidˉwvb|Unsupportedˉprofile|Unsupportedˉmodule|Unsupportedˉfunction|Unsupportedˉcode) /u.test(
            Diagnostic,
        )) {
        Reject(
            `The unsafe-scratch native rejection ${Label} differed: ` +
            `status=${Result.Code} diagnostic=${JSON.stringify(Diagnostic)}.`,
        );
    }
}

async function Requireˉnativeˉexecution(Wvo, Directory, Label) {
    const Checked = await Runˉtool(Nativeˉchecker, [Wvo]);
    if (Checked.Code !== 0 || Checked.Exceeded) {
        Reject(
            `The unsafe-scratch native object check ${Label} differed: ` +
            `status=${Checked.Code} diagnostic=${JSON.stringify(Checked.Diagnostic)}.`,
        );
    }
    const Image = join(Directory, 'Runtime.bin');
    const Linked = await Runˉtool(
        Nativeˉlinker, ['0', 'Main', Image, Wvo],
    );
    const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(
        Linked.Diagnostic.replaceAll('\r\n', '\n'),
    );
    if (Linked.Code !== 0 || Linked.Exceeded || Entry === null || !Exists(Image)) {
        Reject(
            `The unsafe-scratch native link ${Label} differed: ` +
            `status=${Linked.Code} diagnostic=${JSON.stringify(Linked.Diagnostic)}.`,
        );
    }
    const Application = join(
        Directory, process.platform === 'win32' ? 'Runtime.exe' : 'Runtime.elf',
    );
    const Target = process.platform === 'win32'
        ? 'windows-x64-console-v1'
        : 'linux-x64-console-v1';
    const Packaged = await Runˉtool(
        Nativeˉpackager, [Target, Image, Entry[1], Application],
    );
    if (Packaged.Code !== 0 || Packaged.Exceeded || !Exists(Application)) {
        Reject(
            `The unsafe-scratch native package ${Label} differed: ` +
            `status=${Packaged.Code} ` +
            `diagnostic=${JSON.stringify(Packaged.Diagnostic)}.`,
        );
    }
    const Executed = await Runˉtool(Application, []);
    if (Executed.Code !== 42 || Executed.Exceeded ||
        Executed.Diagnostic.length !== 0) {
        Reject(
            `The unsafe-scratch native execution ${Label} differed: ` +
            `status=${Executed.Code} ` +
            `diagnostic=${JSON.stringify(Executed.Diagnostic)}.`,
        );
    }
}

async function Verifyˉnativeˉsplitˉrejections(Wvb, Directory) {
    const Published = await readFile(Wvb);
    const Layout = Inspectˉmemoryˉbudgetˉsplitˉwvb(Published);
    const Cases = [
        {
            Name: 'invalid-split-parent',
            Function: 1,
            Detail: 13,
            Mutate: Candidate => Candidate.writeUInt32LE(
                0xffff_ffff, Layout.Operation + 1,
            ),
        },
        {
            Name: 'invalid-split-result-type',
            Function: 1,
            Detail: 13,
            Mutate: Candidate => Candidate.writeUInt32LE(
                Layout.Typeˉcount, Layout.Operation + 5,
            ),
        },
        {
            Name: 'noncanonical-split-allocation-field',
            Function: 1,
            Detail: 1000,
            Mutate: Candidate => { Candidate[Layout.Allocationˉfieldˉname] ^= 1; },
        },
    ];
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const Case = Cases[Index];
        process.stdout.write(
            'native language 1 unsafe scratch native-split ' +
            `malformed-item=${Index + 1}/${Cases.length} ` +
            `case=${Case.Name} status=Started\n`,
        );
        const Candidate = Buffer.from(Published);
        Case.Mutate(Candidate);
        const Candidateˉpath = join(Directory, `${Case.Name}.wvb`);
        const Objectˉpath = join(Directory, `${Case.Name}.wvo`);
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        await Requireˉnativeˉlowering(
            Candidateˉpath, Objectˉpath, Case.Name, false,
            Case.Detail, Case.Function,
        );
    }
}

async function Verifyˉnativeˉownershipˉrejections(
    Published, Layout, Directory,
) {
    const Takes = [];
    var Cursor = Layout.Codeˉstart;
    while (Cursor < Layout.Codeˉend) {
        const Opcode = Published[Cursor];
        if (Opcode === 205) Takes.push(Cursor);
        Cursor += Unsafeˉscratchˉinstructionˉwidth(Opcode);
    }
    if (Cursor !== Layout.Codeˉend || Takes.length !== 2) {
        Reject('The unsafe-scratch ownership instruction trace differs.');
    }
    const Cases = [
        {
            Name: 'copied-affine-result',
            Mutate: Candidate => { Candidate[Takes[0]] = 4; },
        },
        {
            Name: 'duplicate-affine-take',
            Mutate: Candidate => Candidate.writeUInt32LE(
                Candidate.readUInt32LE(Takes[0] + 1), Takes[1] + 1,
            ),
        },
    ];
    for (const Case of Cases) {
        const Candidate = Buffer.from(Published);
        Case.Mutate(Candidate);
        const Wvb = join(Directory, `${Case.Name}.wvb`);
        const Wvo = join(Directory, `${Case.Name}.wvo`);
        await writeFile(Wvb, Candidate, { flag: 'wx' });
        await Requireˉwvbˉverification(Wvb, false, Case.Name);
        await Requireˉnativeˉlowering(Wvb, Wvo, Case.Name, false, 13);
    }
}

function Unsafeˉscratchˉinstructionˉwidth(Opcode) {
    if (Opcode === 81) return 1;
    if (Opcode === 129 || Opcode === 152) return 9;
    if (Opcode === 220) return 13;
    if ([1, 4, 5, 48, 49, 205].includes(Opcode)) return 5;
    Reject(`The unsafe-scratch ownership trace has opcode ${Opcode}.`);
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
        Codeˉstart: Code.Start,
        Codeˉend: Code.End,
        Operation: Matches[0].Operation,
        Resultˉtype: Matches[0].Resultˉtype,
        Abiˉtype: Matches[0].Abiˉtype,
        Resultˉname: Resultˉdescriptor + 5,
        Typeˉcount,
        Typeˉkinds,
    };
}

function Inspectˉborrowedˉbudgetˉwvb(Input) {
    if (Input.length < 12 || Input.length > MAXIMUM_WVB_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 34 ||
        Input.readUInt32LE(8) !== 7) {
        Reject('The borrowed-budget WVB header differs.');
    }
    const Sections = new Map();
    var Cursor = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Cursor > Input.length - 8 || Input[Cursor] !== Kind ||
            Input[Cursor + 1] !== 0 || Input.readUInt16LE(Cursor + 2) !== 0) {
            Reject('The borrowed-budget WVB section envelope differs.');
        }
        const Length = Input.readUInt32LE(Cursor + 4);
        const Start = Cursor + 8;
        if (Start > Input.length || Length > Input.length - Start) {
            Reject('The borrowed-budget WVB section exceeds the file.');
        }
        Sections.set(Kind, { Start, End: Start + Length });
        Cursor = Start + Length;
    }
    if (Cursor !== Input.length) {
        Reject('The borrowed-budget WVB has trailing bytes.');
    }

    const Functions = Sections.get(4);
    const Code = Sections.get(5);
    const Functionˉcount = Checkedˉu32(Input, Functions.Start, Functions.End);
    if (Functionˉcount === 0 || Functionˉcount > 65_536) {
        Reject('The borrowed-budget WVB function count is invalid.');
    }
    const Entries = [];
    const Borrowedˉparameters = [];
    const Borrowedˉlocals = [];
    Cursor = Functions.Start + 4;
    for (let Functionˉindex = 0;
        Functionˉindex < Functionˉcount; Functionˉindex += 1) {
        Cursor = Checkedˉstring(Input, Cursor, Functions.End);
        const Parameterˉcount = Checkedˉu32(Input, Cursor, Functions.End);
        Cursor += 4;
        if (Parameterˉcount > 64) {
            Reject('The borrowed-budget WVB parameter count is oversized.');
        }
        const Shapes = [];
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            const Shape = Cursor;
            Cursor = Checkedˉshape(Input, Cursor, Functions.End);
            Shapes.push({ Offset: Shape, Kind: Input[Shape] });
            if (Input[Shape] === 36) {
                Borrowedˉparameters.push({
                    Function: Functionˉindex,
                    Local: Parameter,
                    Offset: Shape,
                });
            }
        }
        const Return = Cursor;
        Cursor = Checkedˉshape(Input, Cursor, Functions.End);
        const Localˉcount = Checkedˉu32(Input, Cursor, Functions.End);
        Cursor += 4;
        if (Localˉcount > 2048 - Parameterˉcount) {
            Reject('The borrowed-budget WVB local count is oversized.');
        }
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            const Shape = Cursor;
            Cursor = Checkedˉshape(Input, Cursor, Functions.End);
            Shapes.push({ Offset: Shape, Kind: Input[Shape] });
            if (Input[Shape] === 36) {
                Borrowedˉlocals.push({
                    Function: Functionˉindex,
                    Local: Parameterˉcount + Local,
                    Offset: Shape,
                });
            }
        }
        Cursor = Checkedˉadvance(Cursor, 12, Functions.End);
        const Codeˉoffset = Input.readUInt32LE(Cursor - 12);
        const Codeˉlength = Input.readUInt32LE(Cursor - 8);
        if (Codeˉoffset > Code.End - Code.Start ||
            Codeˉlength > Code.End - Code.Start - Codeˉoffset) {
            Reject('The borrowed-budget WVB function code range is invalid.');
        }
        Entries.push({
            Parameterˉcount,
            Return,
            Shapes,
            Codeˉstart: Code.Start + Codeˉoffset,
            Codeˉend: Code.Start + Codeˉoffset + Codeˉlength,
        });
    }
    if (Cursor !== Functions.End || Borrowedˉparameters.length !== 1 ||
        Borrowedˉlocals.length !== 1) {
        Reject('The borrowed-budget WVB shape directory differs.');
    }
    const Parameter = Borrowedˉparameters[0];
    const View = Borrowedˉlocals[0];
    const Function = Entries[View.Function];
    const Sequences = [];
    for (Cursor = Function.Codeˉstart;
        Cursor <= Function.Codeˉend - 20; Cursor += 1) {
        if (Input[Cursor] !== 4 || Input[Cursor + 5] !== 5 ||
            Input.readUInt32LE(Cursor + 6) !== View.Local ||
            Input[Cursor + 10] !== 4 ||
            Input.readUInt32LE(Cursor + 11) !== View.Local ||
            Input[Cursor + 15] !== 64 ||
            Input.readUInt32LE(Cursor + 16) !== Parameter.Function) {
            continue;
        }
        const Owner = Input.readUInt32LE(Cursor + 1);
        if (Owner >= Function.Shapes.length ||
            Function.Shapes[Owner].Kind !== 25) {
            continue;
        }
        Sequences.push({ Owner, Start: Cursor });
    }
    if (Sequences.length !== 1) {
        Reject('The borrowed-budget WVB transfer sequence differs.');
    }
    return {
        Parameterˉshape: Parameter.Offset,
        Parameterˉreturn: Entries[Parameter.Function].Return,
        Viewˉshape: View.Offset,
        Ownerˉload: Sequences[0].Start,
        Viewˉload: Sequences[0].Start + 10,
    };
}

async function Verifyˉborrowedˉbudgetˉwvbˉrejections(Published, Directory) {
    const Bytes = await readFile(Published);
    const Layout = Inspectˉborrowedˉbudgetˉwvb(Bytes);
    const Cases = [
        {
            Name: 'borrowed-budget-old-minor',
            Mutate: Candidate => Candidate.writeUInt16LE(33, 6),
        },
        {
            Name: 'borrowed-budget-parameter-owner-shape',
            Mutate: Candidate => { Candidate[Layout.Parameterˉshape] = 25; },
        },
        {
            Name: 'borrowed-budget-view-owner-shape',
            Mutate: Candidate => { Candidate[Layout.Viewˉshape] = 25; },
        },
        {
            Name: 'borrowed-budget-unknown-view-shape',
            Mutate: Candidate => { Candidate[Layout.Viewˉshape] = 37; },
        },
        {
            Name: 'borrowed-budget-return-view',
            Mutate: Candidate => { Candidate[Layout.Parameterˉreturn] = 36; },
        },
        {
            Name: 'borrowed-budget-view-take',
            Mutate: Candidate => { Candidate[Layout.Viewˉload] = 205; },
        },
    ];
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const Case = Cases[Index];
        process.stdout.write(
            'native language 1 borrowed budget WVB ' +
            `malformed-item=${Index + 1}/${Cases.length} ` +
            `case=${Case.Name} status=Started\n`,
        );
        const Candidate = Buffer.from(Bytes);
        Case.Mutate(Candidate);
        const Candidateˉpath = join(Directory, `${Case.Name}.wvb`);
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        await Requireˉwvbˉverification(Candidateˉpath, false, Case.Name);
        if (Lowerer !== undefined) {
            await Requireˉnativeˉrejection(
                Candidateˉpath,
                join(Directory, `${Case.Name}.wvo`),
                Case.Name,
            );
        }
    }
    return Cases.length;
}

function Inspectˉmemoryˉbudgetˉsplitˉwvb(Input) {
    const Layout = Inspectˉunsafeˉscratchˉwvb(Input);
    const Matches = [];
    for (let Cursor = Layout.Codeˉstart;
        Cursor <= Layout.Codeˉend - 9; Cursor += 1) {
        if (Input[Cursor] !== 206) continue;
        const Resultˉtype = Input.readUInt32LE(Cursor + 5);
        if (Resultˉtype < Layout.Typeˉcount &&
            Layout.Typeˉkinds[Resultˉtype] === 3) {
            Matches.push(Cursor);
        }
    }
    if (Matches.length !== 1) {
        Reject('The memory-budget split WVB must contain one exact opcode 206.');
    }
    return { ...Layout, Operation: Matches[0] };
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
        if (process.platform === 'win32' && [Tool, ...Arguments].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument)
        )) {
            Rejectˉpromise(new Error(
                'An unsafe-scratch Windows tool argument contains shell metacharacters.',
            ));
            return;
        }
        const Isˉcommand = process.platform === 'win32' &&
            Tool.toLowerCase().endsWith('.cmd');
        const Executable = Isˉcommand
            ? process.env.ComSpec ?? 'cmd.exe'
            : Tool;
        const Toolˉarguments = Isˉcommand
            ? [
                '/d', '/v:off', '/s', '/c',
                `"${[Tool, ...Arguments]
                    .map(Argument => `"${Argument}"`)
                    .join(' ')}"`,
            ]
            : Arguments;
        const Child = spawn(Executable, Toolˉarguments, {
            cwd: Work,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: Isˉcommand,
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
        '<analyzer> <emitter> <compiler-verifier> ' +
        '[runner [native-lowerer]]\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
