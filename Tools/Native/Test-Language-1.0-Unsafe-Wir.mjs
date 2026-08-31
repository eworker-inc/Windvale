import { spawn } from 'node:child_process';
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

const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_ANALYZER_BYTES = 134_217_728;
const MAXIMUM_WIR_BYTES = 4_194_304;
const ANALYSIS_TIMEOUT_MILLISECONDS = 120_000;
const SYSTEM_HEADER =
    'profile system; platform linux, windows, windvale; authority system; ';

if (process.argv.length !== 3) {
    Usage();
}

const Analyzer = resolve(process.argv[2]);
const Analyzerˉstatus = await lstat(Analyzer);
if (!Analyzerˉstatus.isFile() || Analyzerˉstatus.size <= 0 ||
    Analyzerˉstatus.size > MAXIMUM_ANALYZER_BYTES ||
    await realpath(Analyzer) !== Analyzer) {
    Reject('The unsafe-WIR analyzer must be an ordinary canonical file.');
}

const Cases = [
    {
        Name: 'unsafe-expression',
        Expected: 'valid',
        Source:
            'module Unsafeˉexpression; ' + SYSTEM_HEADER +
            'unsafe fn Read() -> i32 effects() { return 42; } ' +
            'export fn Main() -> i32 { return unsafe { Read() }; }',
    },
    {
        Name: 'unsafe-statement',
        Expected: 'valid',
        Source:
            'module Unsafeˉstatement; ' + SYSTEM_HEADER +
            'unsafe fn Read() -> i32 effects() { return 42; } ' +
            'export fn Main() -> i32 { var Value: i32 = 0; ' +
            'unsafe { Value = Read(); } return Value; }',
    },
    {
        Name: 'nested-unsafe-expression',
        Expected: 'valid',
        Source:
            'module Unsafeˉnested; ' + SYSTEM_HEADER +
            'unsafe fn Read() -> i32 effects() { return 42; } ' +
            'export fn Main() -> i32 { return unsafe { unsafe { Read() } }; }',
    },
    {
        Name: 'direct-call-outside-context',
        Expected: 'unsafe-context-required',
        Source:
            'module Unsafeˉdirectˉrejected; ' + SYSTEM_HEADER +
            'unsafe fn Read() -> i32 effects() { return 42; } ' +
            'export fn Main() -> i32 { return Read(); }',
    },
    {
        Name: 'unsafe-declaration-is-not-context',
        Expected: 'unsafe-context-required',
        Source:
            'module Unsafeˉdeclarationˉnotˉcontext; ' + SYSTEM_HEADER +
            'unsafe fn Read() -> i32 effects() { return 42; } ' +
            'unsafe fn Wrapper() -> i32 { return Read(); } ' +
            'export fn Main() -> i32 { return 0; }',
    },
    {
        Name: 'explicit-wrapper',
        Expected: 'valid',
        Source:
            'module Unsafeˉexplicitˉwrapper; ' + SYSTEM_HEADER +
            'unsafe fn Read() -> i32 effects() { return 42; } ' +
            'unsafe fn Wrapper() -> i32 { return unsafe { Read() }; } ' +
            'export fn Main() -> i32 { return unsafe { Wrapper() }; }',
    },
    {
        Name: 'indirect-call-outside-context',
        Expected: 'unsafe-context-required',
        Source:
            'module Unsafeˉindirectˉrejected; ' + SYSTEM_HEADER +
            'unsafe fn Read() -> i32 effects() { return 42; } ' +
            'export fn Main() -> i32 { let Work = Read; return Work(); }',
    },
    {
        Name: 'indirect-call-inside-context',
        Expected: 'valid',
        Source:
            'module Unsafeˉindirectˉaccepted; ' + SYSTEM_HEADER +
            'unsafe fn Read() -> i32 effects() { return 42; } ' +
            'export fn Main() -> i32 { let Work = Read; ' +
            'return unsafe { Work() }; }',
    },
    {
        Name: 'local-value-block',
        Expected: 'valid',
        Source:
            'module Unsafeˉlocalˉvalue; ' + SYSTEM_HEADER +
            'export fn Main() -> i32 { return unsafe { ' +
            'let Value: i32 = 41; Value + 1 }; }',
    },
    {
        Name: 'safe-call',
        Expected: 'valid',
        Source:
            'module Safeˉcall; ' + SYSTEM_HEADER +
            'fn Read() -> i32 { return 42; } ' +
            'export fn Main() -> i32 { return Read(); }',
    },
    {
        Name: 'safe-transparent',
        Expected: 'valid',
        Source:
            'module Safeˉtransparent; ' + SYSTEM_HEADER +
            'export fn Main() -> i32 { return 42; }',
    },
    {
        Name: 'unsafe-expression-transparent',
        Expected: 'valid',
        Source:
            'module Unsafeˉtransparent; ' + SYSTEM_HEADER +
            'export fn Main() -> i32 { return unsafe { 42 }; }',
    },
    {
        Name: 'unsafe-statement-transparent',
        Expected: 'valid',
        Source:
            'module Unsafeˉstatementˉtransparent; ' + SYSTEM_HEADER +
            'export fn Main() -> i32 { unsafe { return 42; } }',
    },
];

const Work = await mkdtemp(join(tmpdir(), 'windvale-unsafe-wir-'));
const Wirˉbyˉname = new Map();
try {
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const Case = Cases[Index];
        const Caseˉdirectory = join(
            Work, `${String(Index).padStart(2, '0')}-${Case.Name}`
        );
        await mkdir(Caseˉdirectory);
        const Input = join(Caseˉdirectory, 'Source.wvss');
        const Sourceˉoutput = join(Caseˉdirectory, 'Output.wvss');
        const Manifest = join(Caseˉdirectory, 'Manifest.wvca');
        const Bindings = join(Caseˉdirectory, 'Bindings.wvlb');
        const Wir = join(Caseˉdirectory, 'Wir.wvir');
        await writeFile(Input, Sourceˉset(Case.Source), { flag: 'wx' });
        const Result = await Runˉanalyzer([
            '--internal-source-set', Input,
            Sourceˉoutput, Manifest, Bindings, Wir,
        ]);
        if (Case.Expected === 'valid') {
            if (Result.Code !== 0 || Result.Exceeded) {
                Reject(
                    `Unsafe-WIR case ${Case.Name} failed with status ` +
                    `${Result.Code}.\n${Result.Diagnostic}`
                );
            }
            const Wirˉbytes = await readFile(Wir);
            if (Wirˉbytes.length < 48 ||
                Wirˉbytes.length > MAXIMUM_WIR_BYTES ||
                Wirˉbytes.subarray(0, 4).toString('ascii') !== 'WVIR') {
                Reject(`Unsafe-WIR case ${Case.Name} published invalid WIR.`);
            }
            Wirˉbyˉname.set(Case.Name, Wirˉbytes);
        } else {
            if (Result.Code !== 1 || Result.Exceeded ||
                !Result.Diagnostic.includes(
                    'wir-status=Unsafeˉcontextˉrequired'
                )) {
                Reject(
                    `Unsafe-WIR rejection ${Case.Name} differed.\n` +
                    Result.Diagnostic
                );
            }
        }
    }
    const Safe = Wirˉcounts(Wirˉbyˉname.get('safe-transparent'));
    const Unsafeˉexpression = Wirˉcounts(
        Wirˉbyˉname.get('unsafe-expression-transparent')
    );
    const Unsafeˉstatement = Wirˉcounts(
        Wirˉbyˉname.get('unsafe-statement-transparent')
    );
    if (!Countsˉequal(Safe, Unsafeˉexpression) ||
        !Countsˉequal(Safe, Unsafeˉstatement)) {
        Reject('Unsafe lexical wrappers emitted an observable WIR operation.');
    }
    process.stdout.write(
        'native language 1 unsafe WIR status=Passed cases=13 ' +
        'valid=10 rejected=3 transparency=2\n'
    );
} finally {
    await Removeˉwork(Work);
}

async function Removeˉwork(Path) {
    const Temporaryˉroot = await realpath(resolve(tmpdir()));
    const Parent = await realpath(dirname(Path));
    if (Parent !== Temporaryˉroot ||
        !basename(Path).startsWith('windvale-unsafe-wir-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Path}`);
    }
    await rm(Path, {
        force: false,
        maxRetries: 2,
        recursive: true,
    });
}

function Sourceˉset(Source) {
    const Sourceˉbytes = Buffer.from(Source, 'utf8');
    const Header = Buffer.alloc(36);
    Header.write('WVSS', 0, 'ascii');
    Header.writeUInt16LE(2, 4);
    Header.writeUInt16LE(0, 6);
    Header.writeUInt32LE(1, 8);
    Header.writeUInt32LE(20, 12);
    Header.writeUInt32LE(36, 16);
    Header.writeUInt32LE(Sourceˉbytes.length, 20);
    Header.writeUInt32LE(1, 24);
    Header.writeUInt32LE(1, 28);
    Header.writeUInt32LE(1, 32);
    return Buffer.concat([Header, Sourceˉbytes]);
}

function Wirˉcounts(Wir) {
    if (Wir === undefined) {
        Reject('A transparency case did not publish WIR.');
    }
    return [8, 16, 24, 32, 40].map(Offset => Wir.readUInt32LE(Offset));
}

function Countsˉequal(Left, Right) {
    return Left.length === Right.length &&
        Left.every((Value, Index) => Value === Right[Index]);
}

function Runˉanalyzer(Arguments) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Child = spawn(Analyzer, Arguments, {
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

function Reject(Message) {
    throw new Error(Message);
}

function Usage() {
    process.stderr.write(
        `Usage: node ${basename(process.argv[1])} <wvanalyze.exe>\n`
    );
    process.exit(64);
}
