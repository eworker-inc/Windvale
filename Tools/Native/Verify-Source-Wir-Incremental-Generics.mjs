import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import {
    lstatSync,
    mkdtempSync,
    readFileSync,
    realpathSync,
    rmSync,
} from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_DIAGNOSTIC_BYTES = 16_384;
const MAXIMUM_ARTIFACT_BYTES = 16_777_216;
const TOOL_TIMEOUT_MILLISECONDS = 60_000;

if (process.argv.length !== 4) {
    process.stderr.write(
        'Usage: node Verify-Source-Wir-Incremental-Generics.mjs ' +
        '<analyzer> <scratch-directory>\n',
    );
    process.exit(64);
}

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
const Analyzer = path.resolve(process.argv[2]);
const Scratch = path.resolve(process.argv[3]);
Requireˉordinaryˉfile(Analyzer, 134_217_728, 'analyzer');
Requireˉordinaryˉdirectory(Scratch, 'scratch directory');

const Cases = [
    {
        Name: 'identity',
        Fixture: 'Generic-Identity-Program.wv',
        Report:
            'source analysis status=Published source-bytes=290 ' +
            'manifest-bytes=104 binding-bytes=252 wir-bytes=520\n',
        Wir: { functions: 3, blocks: 2, operations: 8, operands: 6 },
        Artifacts: [
            ['wvss', 290, '2c92b48ef3d5436e4bc1c3e16f05db7e701562030e3d41e764a6dc62bcb0ed05'],
            ['wvca', 104, '677a0809aeade9a6d484f48004b7d56578ffd445e6c891acc93f6584d425b13f'],
            ['wvlb', 252, 'fab51277ec77b25b0ff0d5304b29e71e510c573ff80d3c35474bc8bec4c44b88'],
            ['wvir', 520, 'ac50293df86cfca4e62c617fd44b600f1bca4010b0a86a430b8422e8a4d6403f'],
        ],
    },
    {
        Name: 'multiple',
        Fixture: 'Generic-Multiple-Specializations.wv',
        Report:
            'source analysis status=Published source-bytes=372 ' +
            'manifest-bytes=104 binding-bytes=360 wir-bytes=932\n',
        Wir: { functions: 5, blocks: 6, operations: 14, operands: 12 },
        Artifacts: [
            ['wvss', 372, 'f0d17cc309e10426ad0f3155ffe153eff8a2364ea5a82c3ae9593b08b6ca99aa'],
            ['wvca', 104, '81776c78776257ad2302415f59a6e18e66189777fab99e7b830a419fb0365fa8'],
            ['wvlb', 360, 'ff2d1310af9ce00b3d17ed3784d2584b0645b4c4a8ca6a8006bb33a97d9fb589'],
            ['wvir', 932, '1eed5ab77bad930f367a54fd153f517c1eb747ecae2b829170f416cecc73365f'],
        ],
    },
    {
        Name: 'nested-discovery',
        Fixture: 'Generic-Nested-Specialization-Discovery.wv',
        Report:
            'source analysis status=Published source-bytes=268 ' +
            'manifest-bytes=104 binding-bytes=288 wir-bytes=548\n',
        Wir: { functions: 5, blocks: 3, operations: 5, operands: 5 },
        Artifacts: [
            ['wvss', 268, '7aeeeb593fcd56f6c48d85276f0c353994ad115c4a4437beb5817ccd753dac4e'],
            ['wvca', 104, '228d1723b2550a0637cf24d230d409afb32035a1a038f1e5c93f77996c267f04'],
            ['wvlb', 288, '11df87d6370639b9fa23f23d6d89095b0446ecacb41ddfb4290a1f3c208b299d'],
            ['wvir', 548, 'b9a82f657dcfde890982f77836c8e3e6f5a75c1c52cb8d0ee33c04256a7ce9ad'],
        ],
    },
];

const Work = mkdtempSync(path.join(Scratch, 'wir-incremental-generics-'));
try {
    let Item = 0;
    for (const Case of Cases) {
        Item += 1;
        process.stdout.write(
            `START incremental generic WIR item=${Item}/${Cases.length} ` +
            `case=${Case.Name}\n`,
        );
        const Prefix = path.join(Work, Case.Name);
        const Outputs = Case.Artifacts.map(([Extension]) =>
            `${Prefix}.${Extension}`,
        );
        const Result = spawnSync(Analyzer, [
            path.join(
                Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0',
                Case.Fixture,
            ),
            ...Outputs,
        ], {
            encoding: 'utf8',
            windowsHide: true,
            maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
            timeout: TOOL_TIMEOUT_MILLISECONDS,
        });
        if (Result.error !== undefined || Result.status !== 0 ||
            Result.stdout !== Case.Report || Result.stderr !== '') {
            Reject(
                `The ${Case.Name} analysis differs: status=${Result.status} ` +
                `error=${Result.error?.message ?? ''}\n` +
                `stdout=${Result.stdout ?? ''}stderr=${Result.stderr ?? ''}`,
            );
        }
        for (let Index = 0; Index < Outputs.length; Index += 1) {
            const [Extension, Bytes, Sha256] = Case.Artifacts[Index];
            const Candidate = Outputs[Index];
            Requireˉordinaryˉfile(
                Candidate, MAXIMUM_ARTIFACT_BYTES,
                `${Case.Name} ${Extension}`,
            );
            const Value = readFileSync(Candidate);
            const Actualˉsha256 = Digest(Value);
            if (Value.length !== Bytes || Actualˉsha256 !== Sha256) {
                Reject(
                    `The ${Case.Name} ${Extension} identity differs: ` +
                    `bytes=${Value.length} sha256=${Actualˉsha256}.`,
                );
            }
            if (Extension === 'wvir') {
                Verifyˉwir(Value, Case.Wir, Case.Name);
            }
        }
        process.stdout.write(
            `PASS  incremental generic WIR item=${Item}/${Cases.length} ` +
            `case=${Case.Name}\n`,
        );
    }
} finally {
    const Resolved = path.resolve(Work);
    if (path.dirname(Resolved) !== path.resolve(Scratch) ||
        !path.basename(Resolved).startsWith('wir-incremental-generics-')) {
        Reject(`Refusing to remove unexpected work directory: ${Resolved}.`);
    }
    rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
}

process.stdout.write(
    'incremental generic WIR status=Passed cases=3 exact-artifacts=12\n',
);

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}.`);
    }
}

function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isDirectory() || Information.isSymbolicLink()) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}.`);
    }
}

function Digest(Value) {
    return createHash('sha256').update(Value).digest('hex');
}

function Verifyˉwir(Value, Expected, Label) {
    if (Value.length < 48 || Value.subarray(0, 4).toString('ascii') !== 'WVIR' ||
        Value.readUInt16LE(4) !== 1 || Value.readUInt16LE(6) !== 10 ||
        Value.readUInt32LE(12) !== 48 || Value.readUInt32LE(20) !== 28 ||
        Value.readUInt32LE(28) !== 28 || Value.readUInt32LE(36) !== 4 ||
        Value.readUInt32LE(8) !== Expected.functions ||
        Value.readUInt32LE(16) !== Expected.blocks ||
        Value.readUInt32LE(24) !== Expected.operations ||
        Value.readUInt32LE(32) !== Expected.operands) {
        Reject(`The ${Label} WIR structure differs.`);
    }
    const Directoryˉbytes = 48 + Expected.functions * 48 +
        Expected.blocks * 28 + Expected.operations * 28 + Expected.operands * 4;
    if (Directoryˉbytes > Value.length) {
        Reject(`The ${Label} WIR directory exceeds its artifact.`);
    }
}

function Reject(Message) {
    throw new Error(Message);
}
