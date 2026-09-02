import { spawn } from 'node:child_process';
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
const MAXIMUM_WVB_BYTES = 4_194_304;
const ANALYSIS_TIMEOUT_MILLISECONDS = 120_000;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SYSTEM_HEADER =
    'profile system; platform linux, windows, windvale; authority application; ';

if (process.argv.length < 3 || process.argv.length > 7) Usage();
const Analyzer = await realpath(resolve(process.argv[2]));
const Analyzerˉstatus = await lstat(Analyzer);
if (!Analyzerˉstatus.isFile() || Analyzerˉstatus.size <= 0 ||
    Analyzerˉstatus.size > MAXIMUM_TOOL_BYTES) {
    Reject('The unsafe write-region analyzer must be an ordinary canonical file.');
}
const Emitter = process.argv.length >= 4 ?
    await realpath(resolve(process.argv[3])) : undefined;
const Compilerˉverifier = process.argv.length >= 5 ?
    await realpath(resolve(process.argv[4])) : undefined;
const Runner = process.argv.length >= 6 ?
    await realpath(resolve(process.argv[5])) : undefined;
const Lowerer = process.argv.length === 7 ?
    await realpath(resolve(process.argv[6])) : undefined;
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
var Frontˉdoorˉverifier;
if (Emitter !== undefined) {
    const Emitterˉstatus = await lstat(Emitter);
    if (!Emitterˉstatus.isFile() || Emitterˉstatus.size <= 0 ||
        Emitterˉstatus.size > MAXIMUM_TOOL_BYTES) {
        Reject('The unsafe write-region emitter must be an ordinary canonical file.');
    }
    if (Compilerˉverifier !== undefined) {
        const Compilerˉverifierˉstatus = await lstat(Compilerˉverifier);
        if (!Compilerˉverifierˉstatus.isFile() ||
            Compilerˉverifierˉstatus.size <= 0 ||
            Compilerˉverifierˉstatus.size > MAXIMUM_TOOL_BYTES) {
            Reject(
                'The unsafe write-region compiler verifier must be an ' +
                'ordinary canonical file.',
            );
        }
    }
    if (Runner !== undefined) {
        const Runnerˉstatus = await lstat(Runner);
        if (!Runnerˉstatus.isFile() || Runnerˉstatus.size <= 0 ||
            Runnerˉstatus.size > MAXIMUM_TOOL_BYTES) {
            Reject(
                'The unsafe write-region runner must be an ordinary ' +
                'canonical file.',
            );
        }
    }
    for (const [Tool, Label] of [
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
                `The unsafe write-region ${Label} must be an ordinary ` +
                'canonical file.',
            );
        }
    }
    const Verifierˉname = process.platform === 'win32' ?
        'wvverify.exe' : 'wvverify.elf';
    Frontˉdoorˉverifier = await realpath(join(
        REPOSITORY_ROOT, 'Artifacts', 'Native-Front-Door',
        process.platform === 'win32' ? 'windows-x64' : 'linux-x64',
        Verifierˉname,
    ));
    const Verifierˉstatus = await lstat(Frontˉdoorˉverifier);
    if (!Verifierˉstatus.isFile() || Verifierˉstatus.size <= 0 ||
        Verifierˉstatus.size > MAXIMUM_TOOL_BYTES) {
        Reject('The front-door verifier must be an ordinary canonical file.');
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
const Scratchˉtype = 'Unsafe.Foreignˉscratch<Hostˉabi>';
const Regionˉtype = 'Unsafe.Foreignˉwriteˉregion<Hostˉabi>';
const Pointerˉfailure = 'Unsafe.Foreignˉpointerˉfailure';
const Regionˉresult =
    'Result.Result<' + Regionˉtype + ', ' + Pointerˉfailure + '>';
const Scratchˉresult =
    'Result.Result<' + Scratchˉtype +
    ', Unsafe.Foreignˉmemoryˉfailure>';
const Validˉcall =
    'Unsafe.Borrowˉwriteˉregion::<Hostˉabi>(' +
    'Scratch: borrow mut Scratch, Start: 0u64, Length: 64u64, ' +
    'Requiredˉalignment: 8u64)';

function Application(
    Scratchˉparameter,
    Call = Validˉcall,
    Resultˉtype = Regionˉresult,
    Unsafeˉcontext = true,
    Prelude = '',
) {
    const Body = Unsafeˉcontext ?
        'unsafe { let Outcome: ' + Resultˉtype + ' = ' + Call +
            '; return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 42 } ' +
            'case Result.Result.Failure { Error: _ } { 1 } }; }' :
        'let Outcome: ' + Resultˉtype + ' = ' + Call +
            '; return match Outcome { ' +
            'case Result.Result.Valid { Value: _ } { 42 } ' +
            'case Result.Result.Failure { Error: _ } { 1 } };';
    return 'module Languageˉoneˉunsafeˉwriteˉregion; ' + SYSTEM_HEADER +
        Imports + Abi + 'export fn Borrow(' + Scratchˉparameter +
        ') -> i32 effects(unsafe.address) { ' + Prelude + Body + ' } ' +
        'export fn Main() -> i32 { return 42; }';
}

function Typeˉapplication() {
    return 'module Languageˉoneˉunsafeˉwriteˉregionˉtype; ' +
        SYSTEM_HEADER + Imports + Abi + 'export fn Accept(Value: ' +
        Regionˉresult + ') -> i32 { return 42; } ' +
        'export fn Main() -> i32 { return 42; }';
}

function Escapingˉapplication() {
    return 'module Languageˉoneˉunsafeˉwriteˉregionˉescape; ' +
        SYSTEM_HEADER + Imports + Abi + 'export fn Escape(Scratch: borrow mut ' +
        Scratchˉtype + ') -> ' + Regionˉresult +
        ' effects(unsafe.address) { unsafe { return ' + Validˉcall + '; } } ' +
        'export fn Main() -> i32 { return 42; }';
}

function Reusingˉscratchˉapplication() {
    return 'module Languageˉoneˉunsafeˉwriteˉregionˉreuse; ' +
        SYSTEM_HEADER + Imports + Abi + 'export fn Reuse(Scratch: borrow mut ' +
        Scratchˉtype + ') -> i32 effects(unsafe.address) { unsafe { ' +
        'let Outcome: ' + Regionˉresult + ' = ' + Validˉcall + '; ' +
        'let Observed: u64 = Unsafe.Scratchˉlength::<Hostˉabi>(' +
        'Scratch: borrow Scratch); return match Outcome { ' +
        'case Result.Result.Valid { Value: _ } { 42 } ' +
        'case Result.Result.Failure { Error: _ } { 1 } }; } } ' +
        'export fn Main() -> i32 { return 42; }';
}

const Cases = [
    {
        Name: 'valid-canonical-write-region-type',
        Expected: 'valid',
        Typeˉonly: true,
        Source: Typeˉapplication(),
    },
    {
        Name: 'valid-canonical-write-region',
        Expected: 'valid',
        Malformed: true,
        Source: Application('Scratch: borrow mut ' + Scratchˉtype),
    },
    {
        Name: 'valid-mutable-local-write-region',
        Expected: 'valid',
        Source: Application(
            'Scratch: ' + Scratchˉtype,
            Validˉcall.replace('borrow mut Scratch', 'borrow mut Local'),
            Regionˉresult,
            true,
            'var Local: ' + Scratchˉtype + ' = Scratch; ',
        ),
    },
    {
        Name: 'analyzer-accepted-verifier-rejected-region-escape',
        Expected: 'valid',
        Verifierˉrejected: true,
        Source: Escapingˉapplication(),
    },
    {
        Name: 'analyzer-accepted-verifier-rejected-scratch-reuse',
        Expected: 'valid',
        Verifierˉrejected: true,
        Source: Reusingˉscratchˉapplication(),
    },
    {
        Name: 'valid-source-unused-mutable-scratch-parameter',
        Expected: 'valid',
        Writerˉrejected: true,
        Source: 'module Languageˉoneˉunusedˉmutableˉscratch; ' +
            SYSTEM_HEADER + Imports + Abi +
            'export fn Borrow(Scratch: borrow mut ' + Scratchˉtype +
            ') -> i32 { return 42; } ' +
            'export fn Main() -> i32 { return 42; }',
    },
    {
        Name: 'outside-unsafe-context',
        Expected: 'Unsafeˉcontextˉrequired',
        Source: Application(
            'Scratch: borrow mut ' + Scratchˉtype,
            Validˉcall,
            Regionˉresult,
            false,
        ),
    },
    {
        Name: 'immutable-scratch-origin',
        Expected: 'Invalidˉborrow',
        Source: Application('Scratch: borrow ' + Scratchˉtype),
    },
    {
        Name: 'by-value-scratch-origin',
        Expected: 'Invalidˉborrow',
        Source: Application('Scratch: ' + Scratchˉtype),
    },
    {
        Name: 'wrong-result-abi',
        Expected: 'Genericˉresolution',
        Source: Application(
            'Scratch: borrow mut ' + Scratchˉtype,
            Validˉcall,
            'Result.Result<Unsafe.Foreignˉwriteˉregion<Otherˉabi>, ' +
                Pointerˉfailure + '>',
        ),
    },
    {
        Name: 'wrong-result-failure',
        Expected: 'Genericˉresolution',
        Source: Application(
            'Scratch: borrow mut ' + Scratchˉtype,
            Validˉcall,
            'Result.Result<' + Regionˉtype + ', ' +
                'Unsafe.Foreignˉmemoryˉfailure>',
        ),
    },
    {
        Name: 'wrong-explicit-abi',
        Expected: 'Genericˉresolution',
        Source: Application(
            'Scratch: borrow mut ' + Scratchˉtype,
            'Unsafe.Borrowˉwriteˉregion::<Otherˉabi>(' +
                'Scratch: borrow mut Scratch, Start: 0u64, Length: 64u64, ' +
                'Requiredˉalignment: 8u64)',
        ),
    },
    {
        Name: 'wrong-alignment-label',
        Expected: 'Invalidˉargument',
        Source: Application(
            'Scratch: borrow mut ' + Scratchˉtype,
            'Unsafe.Borrowˉwriteˉregion::<Hostˉabi>(' +
                'Scratch: borrow mut Scratch, Start: 0u64, Length: 64u64, ' +
                'Alignment: 8u64)',
        ),
    },
];

const Runtimeˉapplications = [
    {
        Name: 'success',
        Call: Runtimeˉcall(8n, 16n, 8n),
        Summary: '',
    },
    {
        Name: 'zero-length',
        Call: Runtimeˉcall(0n, 0n, 8n),
        Summary: Pointerˉsummary(
            'Outˉofˉrange',
            'Rangeˉstart == 0u64 && Rangeˉlength == 0u64 && ' +
                'Ownerˉlength == 64u64',
        ),
    },
    {
        Name: 'address-overflow',
        Call: Runtimeˉcall(18_446_744_073_709_551_615n, 2n, 8n),
        Summary: Pointerˉsummary(
            'Addressˉoverflow',
            'Overflowˉstart == 18446744073709551615u64 && ' +
                'Overflowˉlength == 2u64 && Bits == 64u32',
        ),
    },
    {
        Name: 'out-of-range',
        Call: Runtimeˉcall(63n, 2n, 8n),
        Summary: Pointerˉsummary(
            'Outˉofˉrange',
            'Rangeˉstart == 63u64 && Rangeˉlength == 2u64 && ' +
                'Ownerˉlength == 64u64',
        ),
    },
    {
        Name: 'misaligned',
        Call: Runtimeˉcall(1n, 1n, 8n),
        Summary: Pointerˉsummary(
            'Misaligned',
            'Alignmentˉstart == 1u64 && Alignment == 8u64',
        ),
    },
];

function Runtimeˉcall(Start, Length, Alignment) {
    return 'Unsafe.Borrowˉwriteˉregion::<Hostˉabi>(' +
        'Scratch: borrow mut Scratch, Start: ' + Start + 'u64, Length: ' +
        Length + 'u64, Requiredˉalignment: ' + Alignment + 'u64)';
}

function Pointerˉsummary(Expected, Condition) {
    const Branch = Name => Expected === Name ?
        'if ' + Condition + ' { 42 } else { 2 }' : '3';
    return 'match Failure { ' +
        'case Unsafe.Foreignˉpointerˉfailure.Null { 3 } ' +
        'case Unsafe.Foreignˉpointerˉfailure.Addressˉoverflow { ' +
        'Start: Overflowˉstart, Length: Overflowˉlength, ' +
        'Addressˉbits: Bits } { ' +
        Branch('Addressˉoverflow') + ' } ' +
        'case Unsafe.Foreignˉpointerˉfailure.Outˉofˉrange { ' +
        'Start: Rangeˉstart, Length: Rangeˉlength, ' +
        'Ownerˉlength: Ownerˉlength } { ' +
        Branch('Outˉofˉrange') + ' } ' +
        'case Unsafe.Foreignˉpointerˉfailure.Misaligned { ' +
        'Start: Alignmentˉstart, Requiredˉalignment: Alignment } { ' +
        Branch('Misaligned') + ' } ' +
        'case Unsafe.Foreignˉpointerˉfailure.Aliasing { 3 } ' +
        'case Unsafe.Foreignˉpointerˉfailure.Lifetimeˉended { 3 } ' +
        'case Unsafe.Foreignˉpointerˉfailure.Unsupportedˉabi { 3 } }';
}

function Runtimeˉapplication(Call, Summary) {
    const Failureˉbranch = Summary.length === 0 ?
        'case Result.Result.Failure { Error: _ } { 3 } ' :
        'case Result.Result.Failure { Error: Failure } { ' +
            Summary + ' } ';
    return 'module Languageˉoneˉunsafeˉwriteˉregionˉruntime; ' +
        SYSTEM_HEADER + Imports + Abi +
        'fn Borrow(Value: ' + Scratchˉtype + ') -> i32 ' +
        'effects(unsafe.address) { var Scratch: ' + Scratchˉtype +
        ' = Value; unsafe { let Outcome: ' + Regionˉresult + ' = ' + Call +
        '; return match Outcome { ' +
        'case Result.Result.Valid { Value: _ } { 42 } ' +
        Failureˉbranch + '}; } } ' +
        'export fn Main(Budget: Memory.Memoryˉbudget) -> i32 ' +
        'effects(memory.allocate, unsafe.address) { ' +
        'let Constructed: ' + Scratchˉresult + ' = ' +
        'Unsafe.Constructˉscratch::<Hostˉabi>(' +
        'Budget: Budget, Length: 64u64, Alignment: 8u64); ' +
        'return match Constructed { ' +
        'case Result.Result.Valid { Value: Scratchˉvalue } { ' +
        'Borrow(Scratchˉvalue) } ' +
        'case Result.Result.Failure { Error: _ } { 4 } }; }';
}

const Work = await mkdtemp(join(tmpdir(), 'windvale-unsafe-write-region-wir-'));
var Valid = 0;
var Rejected = 0;
var Malformed = 0;
var Malformedˉwvb = 0;
var Publishedˉwvb = 0;
var Writerˉrejected = 0;
var Compilerˉverifierˉcases = 0;
var Runtimeˉmalformed = 0;
try {
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const Case = Cases[Index];
        process.stdout.write(
            'native language 1 unsafe write region WVIR ' +
            `item=${Index + 1}/${Cases.length} case=${Case.Name} ` +
            'status=Started\n',
        );
        const Directory = join(
            Work, `${String(Index).padStart(2, '0')}-${Case.Name}`,
        );
        await mkdir(Directory);
        const Input = join(Directory, 'Input.wvss');
        const Source = join(Directory, 'Source.wvss');
        const Manifest = join(Directory, 'Manifest.wvca');
        const Bindings = join(Directory, 'Bindings.wvlb');
        const Wir = join(Directory, 'Wir.wvir');
        await writeFile(Input, Sourceˉset([
            Case.Source, Memoryˉmodule, Resultˉmodule, Unsafeˉmodule,
        ]), { flag: 'wx' });
        const Analysis = await Runˉanalyzer([
            '--internal-source-set', Input,
            Source, Manifest, Bindings, Wir,
        ]);
        if (Case.Expected === 'valid') {
            if (Analysis.Code !== 0 || Analysis.Exceeded) {
                Reject(
                    `Unsafe write-region case ${Case.Name} failed with ` +
                    `status ${Analysis.Code}.\n${Analysis.Diagnostic}`,
                );
            }
            if (Case.Writerˉrejected === true) {
                if (Emitter !== undefined) {
                    const Output = join(Directory, 'Rejected.wvb');
                    const Emission = await Runˉtool(Emitter, [
                        Source, Manifest, Bindings, Wir, Output,
                    ]);
                    if (Emission.Code !== 1 || Emission.Exceeded ||
                        !Emission.Diagnostic.includes(
                            'wvb-status=Unsupportedˉshape',
                        )) {
                        Reject(
                            'The unused mutable scratch parameter did not ' +
                            'remain outside candidate WVB 1.36.\n' +
                            Emission.Diagnostic,
                        );
                    }
                    Writerˉrejected += 1;
                }
            } else if (Case.Typeˉonly !== true) {
                const Wirˉbytes = await readFile(Wir);
                Inspectˉvalidˉwir(Wirˉbytes);
                if (Emitter !== undefined) {
                    const Output = join(Directory, 'Candidate.wvb');
                    const Emission = await Runˉtool(Emitter, [
                        Source, Manifest, Bindings, Wir, Output,
                    ]);
                    if (Emission.Code !== 0 || Emission.Exceeded ||
                        !/^source emission status=Published mode=optimized functions=[0-9]+ code-bytes=[0-9]+ module-bytes=[0-9]+\r?\n$/u.test(
                            Emission.Diagnostic,
                        )) {
                        Reject(
                            'The candidate WVB 1.36 publication differed.\n' +
                            Emission.Diagnostic,
                        );
                    }
                    const Wvbˉbytes = await readFile(Output);
                    if (Case.Verifierˉrejected === true) {
                        if (Wvbˉbytes.length < 12 ||
                            Wvbˉbytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
                            Wvbˉbytes.readUInt16LE(4) !== 1 ||
                            Wvbˉbytes.readUInt16LE(6) !== 36) {
                            Reject('The escaping write-region WVB header differs.');
                        }
                        if (Compilerˉverifier !== undefined) {
                            await Requireˉcompilerˉverification(
                                Output, false, Case.Name,
                            );
                            Compilerˉverifierˉcases += 1;
                        }
                        await Verifyˉexecutionˉremainsˉclosed(Output);
                        Publishedˉwvb += 1;
                        Valid += 1;
                        continue;
                    }
                    const Wvbˉlayout = Inspectˉwriteˉregionˉwvb(Wvbˉbytes);
                    if (Compilerˉverifier !== undefined) {
                        await Requireˉcompilerˉverification(
                            Output, true, Case.Name,
                        );
                        Compilerˉverifierˉcases += 1;
                    }
                    await Verifyˉexecutionˉremainsˉclosed(Output);
                    Publishedˉwvb += 1;
                    if (Case.Malformed === true) {
                        Malformed += await Verifyˉmalformedˉwir(
                            Directory, Source, Manifest, Bindings, Wirˉbytes,
                        );
                        Malformedˉwvb += await Verifyˉmalformedˉwvb(
                            Directory, Wvbˉbytes, Wvbˉlayout,
                        );
                    }
                }
            }
            Valid += 1;
        } else {
            if (Analysis.Code !== 1 || Analysis.Exceeded ||
                !Analysis.Diagnostic.includes(
                    `wir-status=${Case.Expected}`,
                )) {
                Reject(
                    `Unsafe write-region rejection ${Case.Name} differed.\n` +
                    Analysis.Diagnostic,
                );
            }
            Rejected += 1;
        }
    }
    if (Runner !== undefined) {
        Runtimeˉmalformed = await Verifyˉruntime();
    }
    process.stdout.write(
        'native language 1 unsafe write region WVIR status=Passed ' +
        `cases=${Cases.length} valid=${Valid} rejected=${Rejected} ` +
        `malformed-wvir=${Malformed} malformed-wvb=${Malformedˉwvb} ` +
        `operation=188 minors=27,28 wvb=1.36 opcode=222 ` +
        `published-wvb=${Publishedˉwvb} ` +
        `legacy-front-door=Closed scalar-provider=${Runner === undefined ?
            'Notˉrequested' : 'Verified'} ` +
        `native-x64=${Lowerer === undefined ?
            'Notˉrequested' : 'Verified'} ` +
        `runtime-cases=${Runner === undefined ? 0 : Runtimeˉapplications.length} ` +
        `runtime-malformed=${Runtimeˉmalformed} ` +
        `writer-rejected=${Writerˉrejected} ` +
        `validator=${Emitter === undefined ? 'Notˉrequested' : 'Verified'} ` +
        `compiler-verifier=${Compilerˉverifier === undefined ?
            'Notˉrequested' : 'Verified'} ` +
        `compiler-verifier-cases=${Compilerˉverifierˉcases}\n`,
    );
} finally {
    await Removeˉwork(Work);
}

async function Verifyˉruntime() {
    const Directory = join(Work, 'runtime');
    await mkdir(Directory);
    var Malformed = 0;
    for (let Index = 0; Index < Runtimeˉapplications.length; Index += 1) {
        const Case = Runtimeˉapplications[Index];
        process.stdout.write(
            'native language 1 unsafe write region runtime ' +
            `item=${Index + 1}/${Runtimeˉapplications.length} ` +
            `case=${Case.Name} status=Started\n`,
        );
        const Caseˉdirectory = join(Directory, Case.Name);
        await mkdir(Caseˉdirectory);
        const Input = join(Caseˉdirectory, 'Source.wvss');
        const Source = join(Caseˉdirectory, 'Output.wvss');
        const Manifest = join(Caseˉdirectory, 'Manifest.wvca');
        const Bindings = join(Caseˉdirectory, 'Bindings.wvlb');
        const Wir = join(Caseˉdirectory, 'Wir.wvir');
        const Wvb = join(Caseˉdirectory, 'Runtime.wvb');
        await writeFile(Input, Sourceˉset([
            Runtimeˉapplication(Case.Call, Case.Summary),
            Memoryˉmodule,
            Resultˉmodule,
            Unsafeˉmodule,
        ]), { flag: 'wx' });
        const Analysis = await Runˉanalyzer([
            '--internal-source-set', Input,
            Source, Manifest, Bindings, Wir,
        ]);
        if (Analysis.Code !== 0 || Analysis.Exceeded) {
            Reject(
                `Unsafe write-region runtime ${Case.Name} analysis failed: ` +
                `status=${Analysis.Code}.\n${Analysis.Diagnostic}`,
            );
        }
        const Emission = await Runˉtool(Emitter, [
            Source, Manifest, Bindings, Wir, Wvb,
        ]);
        if (Emission.Code !== 0 || Emission.Exceeded) {
            Reject(
                `Unsafe write-region runtime ${Case.Name} emission failed: ` +
                `status=${Emission.Code}.\n${Emission.Diagnostic}`,
            );
        }
        await Requireˉcompilerˉverification(
            Wvb, true, `runtime-${Case.Name}`,
        );
        Compilerˉverifierˉcases += 1;
        if (Lowerer !== undefined) {
            const Wvo = join(Caseˉdirectory, 'Runtime.wvo');
            await Requireˉnativeˉlowering(Wvb, Wvo, Case.Name, true);
            await Requireˉnativeˉexecution(Wvo, Caseˉdirectory, Case.Name);
        }
        const Execution = await Runˉtool(Runner, [Wvb]);
        if (Execution.Code !== 0 || Execution.Exceeded ||
            Execution.Diagnostic.replaceAll('\r\n', '\n') !==
                'Result: 42\n') {
            Reject(
                `Unsafe write-region runtime ${Case.Name} execution ` +
                `differed: status=${Execution.Code} ` +
                `diagnostic=${JSON.stringify(Execution.Diagnostic)}.`,
            );
        }
        if (Index === 0) {
            const Candidate = await readFile(Wvb);
            const Layout = Inspectˉwriteˉregionˉwvb(Candidate);
            Candidate[Layout.Operation] = 209;
            const Missingˉoperation = join(
                Caseˉdirectory, 'Missing-Write-Region-Operation.wvb',
            );
            await writeFile(Missingˉoperation, Candidate, { flag: 'wx' });
            const Rejected = await Runˉtool(Runner, [Missingˉoperation]);
            if (Rejected.Code !== 1 || Rejected.Exceeded ||
                Rejected.Diagnostic.replaceAll('\r\n', '\n') !==
                    'wvb run status=Invalid phase=compiler-verification\n') {
                Reject(
                    'Unsafe write-region missing-operation runtime ' +
                    `rejection differed: status=${Rejected.Code} ` +
                    `diagnostic=${JSON.stringify(Rejected.Diagnostic)}.`,
                );
            }
            if (Lowerer !== undefined) {
                await Requireˉnativeˉlowering(
                    Missingˉoperation,
                    join(Caseˉdirectory, 'Missing-Write-Region-Operation.wvo'),
                    'missing-write-region-operation',
                    false,
                );
            }
            Malformed += 1;
        }
    }
    process.stdout.write(
        'native language 1 unsafe write region runtime status=Passed ' +
        `cases=${Runtimeˉapplications.length} malformed=${Malformed} ` +
        `native-aot=${Lowerer === undefined ? 'Notˉrequested' :
            Runtimeˉapplications.length + '/' + Runtimeˉapplications.length} ` +
        `native-execution=${Lowerer === undefined ? 'Notˉrequested' :
            Runtimeˉapplications.length + '/' + Runtimeˉapplications.length} ` +
        `native-rejections=${Lowerer === undefined ? 'Notˉrequested' : 1} ` +
        'result=42 allocation=bounded teardown=bounded\n',
    );
    return Malformed;
}

async function Requireˉnativeˉlowering(Wvb, Wvo, Label, Valid) {
    const Result = await Runˉtool(Lowerer, [Wvb, Wvo]);
    const Diagnostic = Result.Diagnostic.replaceAll('\r\n', '\n');
    if (Result.Exceeded) {
        Reject(`The unsafe write-region native lowerer ${Label} exceeded its bounds.`);
    }
    if (Valid) {
        if (Result.Code !== 0 || !Exists(Wvo) ||
            !/^native x64 status=Valid abi=22 code-bytes=[0-9]+ object-bytes=[0-9]+\n$/u.test(
                Diagnostic,
            )) {
            Reject(
                `The unsafe write-region native lowering ${Label} differed: ` +
                `status=${Result.Code} diagnostic=${JSON.stringify(Diagnostic)}.`,
            );
        }
        return;
    }
    if (Result.Code !== 1 || Exists(Wvo) ||
        !/^native x64 status=(?:Invalidˉwvb|Unsupportedˉprofile|Unsupportedˉmodule|Unsupportedˉfunction|Unsupportedˉcode) /u.test(
            Diagnostic,
        )) {
        Reject(
            `The unsafe write-region native rejection ${Label} differed: ` +
            `status=${Result.Code} diagnostic=${JSON.stringify(Diagnostic)}.`,
        );
    }
}

async function Requireˉnativeˉexecution(Wvo, Directory, Label) {
    const Checked = await Runˉtool(Nativeˉchecker, [Wvo]);
    if (Checked.Code !== 0 || Checked.Exceeded) {
        Reject(
            `The unsafe write-region native object check ${Label} differed: ` +
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
            `The unsafe write-region native link ${Label} differed: ` +
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
            `The unsafe write-region native package ${Label} differed: ` +
            `status=${Packaged.Code} ` +
            `diagnostic=${JSON.stringify(Packaged.Diagnostic)}.`,
        );
    }
    const Executed = await Runˉtool(Application, []);
    if (Executed.Code !== 42 || Executed.Exceeded ||
        Executed.Diagnostic.length !== 0) {
        Reject(
            `The unsafe write-region native execution ${Label} differed: ` +
            `status=${Executed.Code} ` +
            `diagnostic=${JSON.stringify(Executed.Diagnostic)}.`,
        );
    }
}

async function Verifyˉexecutionˉremainsˉclosed(Module) {
    const Result = await Runˉtool(Frontˉdoorˉverifier, [Module]);
    if (Result.Code !== 1 || Result.Exceeded ||
        !/^wvb status=Invalid phase=semantic\r?\n$/u.test(Result.Diagnostic)) {
        Reject(
            'The current execution front door did not remain closed to ' +
            `candidate WVB 1.36.\n${Result.Diagnostic}`,
        );
    }
}

async function Requireˉcompilerˉverification(Module, Valid, Label) {
    const Result = await Runˉtool(Compilerˉverifier, [Module]);
    const Diagnostic = Result.Diagnostic.replaceAll('\r\n', '\n');
    if (Result.Exceeded) {
        Reject(`The write-region compiler verifier ${Label} exceeded its bounds.`);
    }
    if (Valid) {
        if (Result.Code !== 0 || Diagnostic !==
            'wvb status=Valid profile=compiler-aligned\n') {
            Reject(
                `The write-region verifier acceptance ${Label} differed: ` +
                `status=${Result.Code} ` +
                `diagnostic=${JSON.stringify(Diagnostic)}.`,
            );
        }
        return;
    }
    if (Result.Code === 0 || !Diagnostic.includes('wvb status=Invalid')) {
        Reject(
            `The write-region verifier rejection ${Label} differed: ` +
            `status=${Result.Code} diagnostic=${JSON.stringify(Diagnostic)}.`,
        );
    }
}

async function Verifyˉmalformedˉwvb(Directory, Canonical, Layout) {
    const Cases = [
        ['old-minor', Value => Value.writeUInt16LE(35, 6)],
        ['unknown-opcode', Value => { Value[Layout.Operation] = 223; }],
        ['invalid-scratch-local', Value => Value.writeUInt32LE(
            0xffff_ffff, Layout.Operation + 1,
        )],
        ['invalid-result-type', Value => Value.writeUInt32LE(
            Layout.Typeˉcount, Layout.Operation + 5,
        )],
        ['invalid-abi-type', Value => Value.writeUInt32LE(
            Layout.Typeˉcount, Layout.Operation + 9,
        )],
    ];
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const [Name, Mutate] = Cases[Index];
        process.stdout.write(
            'native language 1 unsafe write region WVB ' +
            `item=${Index + 1}/${Cases.length} case=${Name} status=Started\n`,
        );
        const Candidate = Buffer.from(Canonical);
        Mutate(Candidate);
        if (Compilerˉverifier !== undefined) {
            const Candidateˉpath = join(Directory, `Malformed-WVB-${Name}.wvb`);
            await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
            await Requireˉcompilerˉverification(
                Candidateˉpath, false, Name,
            );
            Compilerˉverifierˉcases += 1;
        }
        try {
            Inspectˉwriteˉregionˉwvb(Candidate);
        } catch {
            continue;
        }
        Reject(`The malformed write-region WVB was accepted: ${Name}.`);
    }
    if (Compilerˉverifier !== undefined) {
        const Otherˉfailure = Layout.Typeˉkinds.findIndex(
            (Kind, Index) => Kind === 3 && Index !== Layout.Resultˉtype &&
                Index !== Layout.Pointerˉfailureˉtype,
        );
        if (Otherˉfailure < 0) {
            Reject('The write-region fixture lacks a failure-type forgery.');
        }
        const Semanticˉcases = [
            ['region-equals-scratch', Value => Value.writeUInt32LE(
                Layout.Scratchˉtype, Layout.Regionˉtypeˉoffset,
            )],
            ['wrong-pointer-failure', Value => Value.writeUInt32LE(
                Otherˉfailure, Layout.Pointerˉfailureˉoffset,
            )],
            ['noncanonical-pointer-failure-case', Value => {
                Value[Layout.Pointerˉfailureˉcaseˉname] ^= 1;
            }],
            ['extract-write-region-payload', Value => {
                Value[Layout.Caseˉobservation] = 153;
            }],
            ['extract-write-region-field', Value => {
                Value[Layout.Caseˉobservation] = 196;
            }],
        ];
        for (let Index = 0; Index < Semanticˉcases.length; Index += 1) {
            const [Name, Mutate] = Semanticˉcases[Index];
            process.stdout.write(
                'native language 1 unsafe write region WVB ' +
                `semantic-item=${Index + 1}/${Semanticˉcases.length} ` +
                `case=${Name} status=Started\n`,
            );
            const Candidate = Buffer.from(Canonical);
            Mutate(Candidate);
            const Candidateˉpath = join(
                Directory, `Semantic-WVB-${Name}.wvb`,
            );
            await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
            await Requireˉcompilerˉverification(
                Candidateˉpath, false, Name,
            );
            Compilerˉverifierˉcases += 1;
        }
    }
    return Cases.length;
}

function Inspectˉwriteˉregionˉwvb(Input) {
    if (Input.length < 12 || Input.length > MAXIMUM_WVB_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 36 ||
        Input.readUInt32LE(8) !== 7) {
        Reject('The write-region WVB header differs.');
    }
    const Sections = new Map();
    var Cursor = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Cursor > Input.length - 8 || Input[Cursor] !== Kind ||
            Input[Cursor + 1] !== 0 || Input.readUInt16LE(Cursor + 2) !== 0) {
            Reject('The write-region WVB section envelope differs.');
        }
        const Length = Input.readUInt32LE(Cursor + 4);
        const Start = Cursor + 8;
        if (Start > Input.length || Length > Input.length - Start) {
            Reject('The write-region WVB section exceeds the file.');
        }
        Sections.set(Kind, { Start, End: Start + Length });
        Cursor = Start + Length;
    }
    if (Cursor !== Input.length) {
        Reject('The write-region WVB has trailing bytes.');
    }

    const Types = Sections.get(7);
    const Typeˉcount = Checkedˉwvbˉu32(Input, Types.Start, Types.End);
    if (Typeˉcount === 0 || Typeˉcount > 65_536) {
        Reject('The write-region WVB type count is invalid.');
    }
    const Typeˉkinds = [];
    const Typeˉstarts = [];
    Cursor = Types.Start + 4;
    for (let Index = 0; Index < Typeˉcount; Index += 1) {
        if (Cursor >= Types.End) {
            Reject('The write-region WVB type directory is truncated.');
        }
        Typeˉstarts.push(Cursor);
        Typeˉkinds.push(Input[Cursor]);
        Cursor = Nextˉwvbˉtype(Input, Cursor, Types.End);
    }
    if (Cursor !== Types.End) {
        Reject('The write-region WVB type directory has trailing bytes.');
    }

    const Functions = Sections.get(4);
    const Code = Sections.get(5);
    const Functionˉcount = Checkedˉwvbˉu32(
        Input, Functions.Start, Functions.End,
    );
    if (Functionˉcount === 0 || Functionˉcount > 65_536) {
        Reject('The write-region WVB function count is invalid.');
    }
    const Ranges = [];
    Cursor = Functions.Start + 4;
    for (let Index = 0; Index < Functionˉcount; Index += 1) {
        Cursor = Checkedˉwvbˉstring(Input, Cursor, Functions.End);
        const Parameters = Checkedˉwvbˉu32(Input, Cursor, Functions.End);
        Cursor += 4;
        if (Parameters > 2_048) {
            Reject('The write-region WVB parameter count is oversized.');
        }
        const Shapes = [];
        for (let Parameter = 0; Parameter < Parameters; Parameter += 1) {
            const Shape = Inspectˉwvbˉshape(Input, Cursor, Functions.End);
            Shapes.push(Shape);
            Cursor = Shape.End;
        }
        Cursor = Checkedˉwvbˉshape(Input, Cursor, Functions.End);
        const Locals = Checkedˉwvbˉu32(Input, Cursor, Functions.End);
        Cursor += 4;
        if (Locals > 4_096 - Parameters) {
            Reject('The write-region WVB local count is oversized.');
        }
        for (let Local = 0; Local < Locals; Local += 1) {
            const Shape = Inspectˉwvbˉshape(Input, Cursor, Functions.End);
            Shapes.push(Shape);
            Cursor = Shape.End;
        }
        Checkedˉwvbˉadvance(Cursor, 12, Functions.End);
        const Offset = Input.readUInt32LE(Cursor);
        const Length = Input.readUInt32LE(Cursor + 4);
        Cursor += 12;
        if (Offset > Code.End - Code.Start ||
            Length > Code.End - Code.Start - Offset) {
            Reject('The write-region WVB function code range is invalid.');
        }
        Ranges.push({
            Start: Code.Start + Offset,
            End: Code.Start + Offset + Length,
            Locals: Parameters + Locals,
            Shapes,
        });
    }
    if (Cursor !== Functions.End) {
        Reject('The write-region WVB function directory has trailing bytes.');
    }

    const Matches = [];
    for (Cursor = Code.Start; Cursor <= Code.End - 13; Cursor += 1) {
        if (Input[Cursor] !== 222) continue;
        const Resultˉtype = Input.readUInt32LE(Cursor + 5);
        const Abiˉtype = Input.readUInt32LE(Cursor + 9);
        if (Resultˉtype < Typeˉcount && Abiˉtype < Typeˉcount &&
            Typeˉkinds[Resultˉtype] === 3 &&
            (Typeˉkinds[Abiˉtype] === 2 || Typeˉkinds[Abiˉtype] === 7)) {
            Matches.push({
                Operation: Cursor,
                Scratch: Input.readUInt32LE(Cursor + 1),
                Resultˉtype,
                Abiˉtype,
            });
        }
    }
    if (Matches.length !== 1) {
        Reject('The write-region WVB must contain one exact opcode 222.');
    }
    const Owner = Ranges.find(Range =>
        Matches[0].Operation >= Range.Start &&
        Matches[0].Operation < Range.End,
    );
    if (Owner === undefined || Matches[0].Scratch >= Owner.Locals) {
        Reject('The write-region WVB scratch local is invalid.');
    }
    const Scratchˉshape = Owner.Shapes[Matches[0].Scratch];
    if (Scratchˉshape === undefined ||
        (Scratchˉshape.Kind !== 7 && Scratchˉshape.Kind !== 28) ||
        Scratchˉshape.Nominal >= Typeˉcount) {
        Reject('The write-region WVB scratch type is invalid.');
    }
    const Regionˉlayout = Inspectˉwriteˉregionˉresultˉlayout(
        Input, Typeˉstarts, Matches[0].Resultˉtype, Typeˉcount,
    );
    const Caseˉobservations = [];
    for (Cursor = Owner.Start; Cursor <= Owner.End - 9; Cursor += 1) {
        if (Input[Cursor] === 152 &&
            Input.readUInt32LE(Cursor + 1) === Matches[0].Resultˉtype) {
            Caseˉobservations.push(Cursor);
        }
    }
    if (Caseˉobservations.length !== 1) {
        Reject('The write-region WVB case observation differs.');
    }
    return {
        Operation: Matches[0].Operation,
        Resultˉtype: Matches[0].Resultˉtype,
        Abiˉtype: Matches[0].Abiˉtype,
        Typeˉcount,
        Typeˉkinds,
        Scratchˉtype: Scratchˉshape.Nominal,
        Caseˉobservation: Caseˉobservations[0],
        ...Regionˉlayout,
    };
}

function Inspectˉwriteˉregionˉresultˉlayout(
    Input, Typeˉstarts, Resultˉtype, Typeˉcount,
) {
    var Cursor = Typeˉstarts[Resultˉtype];
    if (Cursor === undefined || Input[Cursor] !== 3) {
        Reject('The write-region Result type is invalid.');
    }
    Cursor = Checkedˉwvbˉstring(Input, Cursor + 1, Input.length);
    if (Input.readUInt32LE(Cursor) !== 2) {
        Reject('The write-region Result cases differ.');
    }
    Cursor += 4;
    Cursor = Checkedˉwvbˉstring(Input, Cursor, Input.length);
    if (Input[Cursor] !== 1) {
        Reject('The write-region valid case differs.');
    }
    Cursor += 1;
    Cursor = Checkedˉwvbˉstring(Input, Cursor, Input.length);
    const Regionˉshape = Inspectˉwvbˉshape(Input, Cursor, Input.length);
    if (Regionˉshape.Kind !== 7 || Regionˉshape.Nominal >= Typeˉcount) {
        Reject('The write-region valid payload differs.');
    }
    const Regionˉtypeˉoffset = Cursor + 1;
    Cursor = Regionˉshape.End;
    Cursor = Checkedˉwvbˉstring(Input, Cursor, Input.length);
    if (Input[Cursor] !== 1) {
        Reject('The write-region failure case differs.');
    }
    Cursor += 1;
    Cursor = Checkedˉwvbˉstring(Input, Cursor, Input.length);
    const Failureˉshape = Inspectˉwvbˉshape(Input, Cursor, Input.length);
    if (Failureˉshape.Kind !== 11 ||
        Failureˉshape.Nominal >= Typeˉcount) {
        Reject('The write-region failure payload differs.');
    }
    const Pointerˉfailureˉoffset = Cursor + 1;
    const Pointerˉfailureˉtype = Failureˉshape.Nominal;
    Cursor = Typeˉstarts[Pointerˉfailureˉtype];
    if (Cursor === undefined || Input[Cursor] !== 3) {
        Reject('The write-region pointer failure type differs.');
    }
    Cursor = Checkedˉwvbˉstring(Input, Cursor + 1, Input.length);
    if (Input.readUInt32LE(Cursor) !== 7) {
        Reject('The write-region pointer failure cases differ.');
    }
    Cursor += 4;
    const Firstˉcaseˉlength = Checkedˉwvbˉu32(
        Input, Cursor, Input.length,
    );
    if (Firstˉcaseˉlength === 0) {
        Reject('The write-region pointer failure case name is empty.');
    }
    return {
        Regionˉtypeˉoffset,
        Pointerˉfailureˉoffset,
        Pointerˉfailureˉtype,
        Pointerˉfailureˉcaseˉname: Cursor + 4,
    };
}

function Nextˉwvbˉtype(Input, Start, End) {
    const Kind = Input[Start];
    var Cursor = Start + 1;
    if (Kind === 8) {
        Cursor = Checkedˉwvbˉadvance(Cursor, 1, End);
        Cursor = Checkedˉwvbˉshape(Input, Cursor, End);
        const Parameters = Checkedˉwvbˉu32(Input, Cursor, End);
        Cursor += 4;
        if (Parameters > 64) {
            Reject('The write-region WVB callable arity is oversized.');
        }
        for (let Index = 0; Index < Parameters; Index += 1) {
            Cursor = Checkedˉwvbˉshape(Input, Cursor, End);
        }
        return Checkedˉwvbˉadvance(Cursor, 10 + Parameters, End);
    }
    if (Kind < 1 || Kind > 7) {
        Reject('The write-region WVB type kind is unknown.');
    }
    Cursor = Checkedˉwvbˉstring(Input, Cursor, End);
    if (Kind === 4) {
        Cursor = Checkedˉwvbˉshape(Input, Cursor, End);
        return Checkedˉwvbˉadvance(Cursor, 4, End);
    }
    if (Kind === 5 || Kind === 6) {
        return Checkedˉwvbˉshape(Input, Cursor, End);
    }
    if (Kind === 7) Cursor = Checkedˉwvbˉadvance(Cursor, 1, End);
    const Items = Checkedˉwvbˉu32(Input, Cursor, End);
    Cursor += 4;
    if (Items > 256) {
        Reject('The write-region WVB type item count is oversized.');
    }
    for (let Item = 0; Item < Items; Item += 1) {
        Cursor = Checkedˉwvbˉstring(Input, Cursor, End);
        if (Kind === 1) {
            Cursor = Checkedˉwvbˉshape(Input, Cursor, End);
        } else if (Kind === 2) {
            Cursor = Checkedˉwvbˉadvance(Cursor, 4, End);
        } else if (Kind === 7) {
            Cursor = Checkedˉwvbˉadvance(Cursor, 1, End);
        } else {
            const Encoding = Input[Cursor];
            Cursor = Checkedˉwvbˉadvance(Cursor, 1, End);
            if (Encoding === 1) {
                Cursor = Checkedˉwvbˉstring(Input, Cursor, End);
                Cursor = Checkedˉwvbˉshape(Input, Cursor, End);
            } else if (Encoding === 2) {
                const Fields = Checkedˉwvbˉu32(Input, Cursor, End);
                Cursor += 4;
                if (Fields < 2 || Fields > 64) {
                    Reject('The write-region WVB variant field count is invalid.');
                }
                for (let Field = 0; Field < Fields; Field += 1) {
                    Cursor = Checkedˉwvbˉstring(Input, Cursor, End);
                    Cursor = Checkedˉwvbˉshape(Input, Cursor, End);
                }
            } else if (Encoding !== 0) {
                Reject('The write-region WVB variant encoding is invalid.');
            }
        }
    }
    return Cursor;
}

function Checkedˉwvbˉshape(Input, Cursor, End) {
    return Inspectˉwvbˉshape(Input, Cursor, End).End;
}

function Inspectˉwvbˉshape(Input, Cursor, End) {
    const Kind = Input[Checkedˉwvbˉadvance(Cursor, 1, End) - 1];
    var Nominal = 0;
    var Shapeˉend = Cursor + 1;
    if ([7, 8, 11, 22, 23, 24, 26, 27, 28, 29, 30, 35].includes(Kind)) {
        Checkedˉwvbˉadvance(Cursor + 1, 4, End);
        Nominal = Input.readUInt32LE(Cursor + 1);
        Shapeˉend += 4;
    }
    return { End: Shapeˉend, Kind, Nominal };
}

function Checkedˉwvbˉstring(Input, Cursor, End) {
    const Length = Checkedˉwvbˉu32(Input, Cursor, End);
    return Checkedˉwvbˉadvance(Cursor + 4, Length, End);
}

function Checkedˉwvbˉu32(Input, Cursor, End) {
    Checkedˉwvbˉadvance(Cursor, 4, End);
    return Input.readUInt32LE(Cursor);
}

function Checkedˉwvbˉadvance(Cursor, Length, End) {
    if (!Number.isSafeInteger(Cursor) || !Number.isSafeInteger(Length) ||
        Cursor < 0 || Length < 0 || Cursor > End || Length > End - Cursor) {
        Reject('The write-region WVB directory is truncated.');
    }
    return Cursor + Length;
}

function Inspectˉvalidˉwir(Input) {
    if (Input.length < 64 || Input.length > MAXIMUM_WIR_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVIR' ||
        Input.readUInt16LE(4) !== 1 ||
        (Input.readUInt16LE(6) !== 27 && Input.readUInt16LE(6) !== 28) ||
        Input.readUInt32LE(12) !== 48 || Input.readUInt32LE(20) !== 28 ||
        Input.readUInt32LE(28) !== 28 || Input.readUInt32LE(36) !== 4 ||
        Input.readUInt32LE(44) !== 4) {
        Reject('The valid write-region analysis did not publish exact WVIR.');
    }
    const Headerˉbytes = Input.readUInt16LE(6) === 28 ? 64 : 56;
    const Functions = Input.readUInt32LE(8);
    const Blocks = Input.readUInt32LE(16);
    const Operations = Input.readUInt32LE(24);
    const Temporaries = Input.readUInt32LE(32);
    const Operands = Input.readUInt32LE(40);
    const Operationsˉoffset = Headerˉbytes + Functions * 48 + Blocks * 28;
    const Temporariesˉoffset = Operationsˉoffset + Operations * 28;
    const Operandsˉoffset = Temporariesˉoffset + Temporaries * 4;
    if (Operandsˉoffset > Input.length ||
        Operands > Math.floor((Input.length - Operandsˉoffset) / 4)) {
        Reject('The write-region WVIR directories exceed the file.');
    }
    const Matches = [];
    for (let Index = 0; Index < Operations; Index += 1) {
        const Entry = Operationsˉoffset + Index * 28;
        if (Input.readUInt16LE(Entry + 4) === 188) Matches.push(Entry);
    }
    if (Matches.length !== 1) {
        Reject('The valid write-region WVIR must contain operation 188 once.');
    }
    const Operation = Matches[0];
    const Operationˉindex = (Operation - Operationsˉoffset) / 28;
    var Localˉlimit = 0;
    for (let Index = 0; Index < Functions; Index += 1) {
        const Function = Headerˉbytes + Index * 48;
        const First = Input.readUInt32LE(Function + 12);
        const Count = Input.readUInt32LE(Function + 16);
        if (Operationˉindex >= First && Operationˉindex - First < Count) {
            Localˉlimit = Input.readUInt32LE(Function + 36) +
                Input.readUInt32LE(Function + 40);
        }
    }
    const Shape = Input.readUInt32LE(Operation + 8);
    const Temporary = Input.readUInt32LE(Operation + 12);
    const Firstˉoperand = Input.readUInt32LE(Operation + 16);
    const Target = Input.readUInt32LE(Operation + 20);
    const Abiˉshape = Input.readUInt32LE(Operation + 24);
    if (Input.readUInt16LE(Operation + 6) !== 3 || Shape < 0x8000_0000 ||
        Temporary >= Temporaries || Firstˉoperand > Operands - 3 ||
        Target >= Localˉlimit || Abiˉshape < 131_072 ||
        Abiˉshape >= 196_608 ||
        Input.readUInt32LE(Temporariesˉoffset + Temporary * 4) !== Shape) {
        Reject('The write-region operation header evidence differs.');
    }
    for (let Index = 0; Index < 3; Index += 1) {
        const Operandˉtemporary = Input.readUInt32LE(
            Operandsˉoffset + (Firstˉoperand + Index) * 4,
        );
        if (Operandˉtemporary >= Temporary ||
            Input.readUInt32LE(
                Temporariesˉoffset + Operandˉtemporary * 4,
            ) !== 8) {
            Reject('The write-region offset/length/alignment evidence differs.');
        }
    }
}

async function Verifyˉmalformedˉwir(
    Directory,
    Source,
    Manifest,
    Bindings,
    Canonical,
) {
    const Operation = Findˉwriteˉregionˉoperation(Canonical);
    const Operands = Canonical.readUInt32LE(40);
    const Mutations = [
        ['lower-minor', Value => Value.writeUInt16LE(26, 6)],
        ['wrong-operation', Value => Value.writeUInt16LE(187, Operation + 4)],
        ['wrong-operand-count', Value => Value.writeUInt16LE(2, Operation + 6)],
        ['wrong-result-shape', Value => Value.writeUInt32LE(8, Operation + 8)],
        ['wrong-first-operand', Value => Value.writeUInt32LE(Operands, Operation + 16)],
        ['wrong-scratch-slot', Value => Value.writeUInt32LE(0xffff_ffff, Operation + 20)],
        ['wrong-abi-shape', Value => Value.writeUInt32LE(3, Operation + 24)],
    ];
    for (let Index = 0; Index < Mutations.length; Index += 1) {
        const [Name, Mutate] = Mutations[Index];
        process.stdout.write(
            'native language 1 unsafe write region malformed ' +
            `item=${Index + 1}/${Mutations.length} case=${Name} ` +
            'status=Started\n',
        );
        const Candidate = Buffer.from(Canonical);
        Mutate(Candidate);
        const Wir = join(Directory, `Malformed-${Name}.wvir`);
        const Output = join(Directory, `Malformed-${Name}.wvb`);
        await writeFile(Wir, Candidate, { flag: 'wx' });
        const Emission = await Runˉtool(Emitter, [
            Source, Manifest, Bindings, Wir, Output,
        ]);
        if (Emission.Code !== 1 || Emission.Exceeded ||
            !Emission.Diagnostic.includes('analysis-status=Invalidˉwir')) {
            Reject(
                `Malformed write-region case ${Name} was not rejected by ` +
                `independent WVIR validation.\n${Emission.Diagnostic}`,
            );
        }
    }
    return Mutations.length;
}

function Findˉwriteˉregionˉoperation(Input) {
    const Headerˉbytes = Input.readUInt16LE(6) === 28 ? 64 : 56;
    const Functions = Input.readUInt32LE(8);
    const Blocks = Input.readUInt32LE(16);
    const Operations = Input.readUInt32LE(24);
    const Operationsˉoffset = Headerˉbytes + Functions * 48 + Blocks * 28;
    var Found = -1;
    for (let Index = 0; Index < Operations; Index += 1) {
        const Entry = Operationsˉoffset + Index * 28;
        if (Input.readUInt16LE(Entry + 4) === 188) {
            if (Found !== -1) {
                Reject('The valid write-region WVIR contains duplicate operation 188.');
            }
            Found = Entry;
        }
    }
    if (Found === -1) {
        Reject('The valid write-region WVIR is missing operation 188.');
    }
    return Found;
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
        Reject('The unsafe write-region source module count is invalid.');
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
    if (process.env.WINDVALE_KEEP_TEST_WORK === '1') {
        process.stdout.write(
            `native language 1 unsafe write region work=${Path}\n`,
        );
        return;
    }
    const Temporaryˉroot = await realpath(resolve(tmpdir()));
    const Parent = await realpath(dirname(Path));
    if (Parent !== Temporaryˉroot ||
        !basename(Path).startsWith('windvale-unsafe-write-region-wir-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Path}`);
    }
    await rm(Path, { force: false, maxRetries: 2, recursive: true });
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/' +
        'Test-Language-1.0-Unsafe-Write-Region-Wir.mjs ' +
        '<analyzer> [emitter] [compiler-verifier] [runner] [native-lowerer]\n',
    );
    process.exit(64);
}

function Exists(Path) {
    return existsSync(Path);
}

function Reject(Message) {
    throw new Error(Message);
}
