import { spawnSync } from 'node:child_process';
import {
    lstatSync,
    readFileSync,
    realpathSync,
    unlinkSync,
    writeFileSync,
} from 'node:fs';
import path from 'node:path';

const MAXIMUM_INPUT_BYTES = 4_194_304;
const MAXIMUM_TOOL_BYTES = 67_108_864;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;

if (process.argv.length !== 8) {
    process.stderr.write(
        'Usage: node Tools/Native/Verify-Language-1.0-Memory-Budget-Split-Wir.mjs ' +
        '<emitter> <source.wvss> <manifest.wvca> <bindings.wvlb> ' +
        '<valid.wvir> <work-directory>\n',
    );
    process.exit(64);
}

const Emitter = path.resolve(process.argv[2]);
const Source = path.resolve(process.argv[3]);
const Manifest = path.resolve(process.argv[4]);
const Bindings = path.resolve(process.argv[5]);
const Validˉpath = path.resolve(process.argv[6]);
const Work = path.resolve(process.argv[7]);

Requireˉordinaryˉfile(Emitter, MAXIMUM_TOOL_BYTES, 'emitter');
Requireˉordinaryˉfile(Source, MAXIMUM_INPUT_BYTES, 'source set');
Requireˉordinaryˉfile(Manifest, MAXIMUM_INPUT_BYTES, 'analysis manifest');
Requireˉordinaryˉfile(Bindings, MAXIMUM_INPUT_BYTES, 'bindings');
Requireˉordinaryˉfile(Validˉpath, MAXIMUM_INPUT_BYTES, 'WVIR');
Requireˉordinaryˉdirectory(Work, 'work directory');

const Valid = readFileSync(Validˉpath);
const Layout = Inspectˉvalidˉlayout(Valid);
const Invalidˉanalysis =
    'source emission status=Invalidˉanalysis analysis-status=Invalidˉwir ' +
    'wvb-status=Sourceˉwir function=0 operation=0 source-line=0\n';
const Unsupportedˉwvb =
    'source emission status=Valid analysis-status=Valid ' +
    'wvb-status=Unsupportedˉshape function=2 operation=6 source-line=0\n';
const Cases = [
    {
        name: 'old-minor',
        mutate: Candidate => Candidate.writeUInt16LE(10, 6),
    },
    {
        name: 'unknown-operation',
        mutate: Candidate => Candidate.writeUInt16LE(
            65_535, Layout.operation + 4,
        ),
    },
    {
        name: 'primitive-result',
        mutate: Candidate => Candidate.writeUInt32LE(
            25, Layout.operation + 8,
        ),
    },
    {
        name: 'missing-limit-operand',
        mutate: Candidate => Candidate.writeUInt16LE(
            1, Layout.operation + 6,
        ),
    },
    {
        name: 'consumed-parent-slot',
        mutate: Candidate => Candidate.writeUInt32LE(
            0, Layout.operation + 20,
        ),
    },
    {
        name: 'wrong-memory-module',
        mutate: Candidate => Candidate.writeUInt32LE(
            2, Layout.operation + 24,
        ),
    },
    {
        name: 'swapped-limits',
        mutate: Candidate => {
            const First = Candidate.readUInt32LE(Layout.firstOperand);
            const Second = Candidate.readUInt32LE(Layout.firstOperand + 4);
            Candidate.writeUInt32LE(Second, Layout.firstOperand);
            Candidate.writeUInt32LE(First, Layout.firstOperand + 4);
        },
    },
];

const Created = [];
try {
    const Validˉoutput = path.join(Work, 'valid-boundary.wvb');
    Created.push(Validˉoutput);
    Requireˉrejection(
        Run(Validˉpath, Validˉoutput), Unsupportedˉwvb,
        'valid WVIR WVB boundary', Validˉoutput,
    );

    for (const Case of Cases) {
        const Candidate = Buffer.from(Valid);
        Case.mutate(Candidate);
        const Candidateˉpath = path.join(Work, `${Case.name}.wvir`);
        const Outputˉpath = path.join(Work, `${Case.name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Created.push(Candidateˉpath, Outputˉpath);
        Requireˉrejection(
            Run(Candidateˉpath, Outputˉpath), Invalidˉanalysis,
            Case.name, Outputˉpath,
        );
    }
    process.stdout.write(
        'language 1 memory budget split WVIR status=Passed ' +
        `valid-boundary=1 malformed=${Cases.length} ` +
        `wvir-bytes=${Valid.length}\n`,
    );
} finally {
    for (const Candidate of Created) {
        try { unlinkSync(Candidate); } catch (Error) {
            if (Error?.code !== 'ENOENT') throw Error;
        }
    }
}

function Inspectˉvalidˉlayout(Input) {
    if (Input.length !== 544 ||
        Input.subarray(0, 4).toString('ascii') !== 'WVIR' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 11 ||
        Input.readUInt32LE(8) !== 5 || Input.readUInt32LE(12) !== 48 ||
        Input.readUInt32LE(16) !== 2 || Input.readUInt32LE(20) !== 28 ||
        Input.readUInt32LE(24) !== 6 || Input.readUInt32LE(28) !== 28 ||
        Input.readUInt32LE(32) !== 5 || Input.readUInt32LE(36) !== 4 ||
        Input.readUInt32LE(40) !== 3 || Input.readUInt32LE(44) !== 4) {
        Reject('The valid Memoryˉbudget.Split fixture is not exact WVIR 1.11.');
    }
    const Blocksˉoffset = 48 + Input.readUInt32LE(8) * 48;
    const Operationsˉoffset = Blocksˉoffset + Input.readUInt32LE(16) * 28;
    const Temporariesˉoffset = Operationsˉoffset +
        Input.readUInt32LE(24) * 28;
    const Operandsˉoffset = Temporariesˉoffset +
        Input.readUInt32LE(32) * 4;
    if (Operandsˉoffset + Input.readUInt32LE(40) * 4 !== Input.length) {
        Reject('The valid WVIR directory length is inconsistent.');
    }
    const Matches = [];
    for (let Index = 0; Index < Input.readUInt32LE(24); Index += 1) {
        const Entry = Operationsˉoffset + Index * 28;
        if (Input.readUInt16LE(Entry + 4) === 171) Matches.push(Entry);
    }
    if (Matches.length !== 1) {
        Reject('The valid WVIR must contain exactly one operation 171.');
    }
    const Operation = Matches[0];
    const Firstˉoperand = Input.readUInt32LE(Operation + 16);
    if (Input.readUInt32LE(Operation + 8) !== 2_147_483_648 ||
        Input.readUInt16LE(Operation + 6) !== 2 ||
        Input.readUInt32LE(Operation + 20) !== 1 ||
        Input.readUInt32LE(Operation + 24) !== 1 || Firstˉoperand !== 1 ||
        Input.readUInt32LE(Temporariesˉoffset + 4) !== 8 ||
        Input.readUInt32LE(Temporariesˉoffset + 8) !== 3 ||
        Input.readUInt32LE(Operandsˉoffset + Firstˉoperand * 4) !== 1 ||
        Input.readUInt32LE(Operandsˉoffset + (Firstˉoperand + 1) * 4) !== 2) {
        Reject('The exact Memoryˉbudget.Split operation evidence differs.');
    }
    return {
        operation: Operation,
        firstOperand: Operandsˉoffset + Firstˉoperand * 4,
    };
}

function Run(Wir, Output) {
    return spawnSync(
        Emitter, [Source, Manifest, Bindings, Wir, Output],
        {
            encoding: 'utf8',
            windowsHide: true,
            maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        },
    );
}

function Requireˉrejection(Result, Expected, Label, Output) {
    if (Result.error !== undefined || Result.status !== 1 ||
        Result.stdout.length !== 0 || Normalize(Result.stderr) !== Expected ||
        Exists(Output)) {
        Reject(
            `The ${Label} rejection differs: status=${Result.status} ` +
            `stdout=${JSON.stringify(Result.stdout)} ` +
            `stderr=${JSON.stringify(Normalize(Result.stderr))}.`,
        );
    }
}

function Exists(Candidate) {
    try { lstatSync(Candidate); return true; } catch (Error) {
        if (Error?.code === 'ENOENT') return false;
        throw Error;
    }
}

function Normalize(Value) {
    return Value.replaceAll('\r\n', '\n');
}

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        !Sameˉpath(realpathSync(Candidate), Candidate)) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}.`);
    }
}

function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isDirectory() || Information.isSymbolicLink() ||
        !Sameˉpath(realpathSync(Candidate), Candidate)) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}.`);
    }
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === path.resolve(Right).toLowerCase()
        : Left === path.resolve(Right);
}

function Reject(Message) {
    throw new Error(Message);
}
