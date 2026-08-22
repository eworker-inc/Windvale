import { readFileSync, unlinkSync, writeFileSync } from 'node:fs';
import { basename, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

function Reject(Message) {
    throw new Error(Message);
}

if (process.argv.length !== 6) {
    process.stderr.write(
        'Usage: node Tools/Native/Verify-Language-1.0-Fixed-Arrays.mjs ' +
        '<verifier> <runner> <valid.wvb> <existing-work-directory>\n',
    );
    process.exit(64);
}

const Verifier = resolve(process.argv[2]);
const Runner = resolve(process.argv[3]);
const Validˉpath = resolve(process.argv[4]);
const Work = resolve(process.argv[5]);
const Mutations = [
    `${Work}/Fixed-Array-Bounds.wvb`,
    `${Work}/Fixed-Array-Old-Minor.wvb`,
    `${Work}/Fixed-Array-Large-Count.wvb`,
    `${Work}/Fixed-Array-Unknown-Type.wvb`,
];

function Parseˉmodule(Path) {
    const Bytes = readFileSync(Path);
    if (Bytes.length < 12 || Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 17 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject(`${basename(Path)} is not a WVB 1.17 seven-section module.`);
    }
    const Sections = [];
    let Cursor = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Cursor + 8 > Bytes.length || Bytes[Cursor] !== Kind) {
            Reject(`${basename(Path)} has no canonical section ${Kind}.`);
        }
        const Length = Bytes.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Bytes.length) {
            Reject(`${basename(Path)} truncates section ${Kind}.`);
        }
        Sections[Kind] = { length: Length, payload: Payload };
        Cursor = Payload + Length;
    }
    if (Cursor !== Bytes.length) {
        Reject(`${basename(Path)} has trailing bytes.`);
    }
    return { Bytes, Sections };
}

function Findˉunique(Bytes, Start, Length, Needle) {
    let Found = -1;
    for (let Offset = Start; Offset + Needle.length <= Start + Length; Offset += 1) {
        if (Bytes.subarray(Offset, Offset + Needle.length).equals(Needle)) {
            if (Found >= 0) {
                Reject('The fixed-array fixture contains a repeated mutation target.');
            }
            Found = Offset;
        }
    }
    if (Found < 0) {
        Reject('The fixed-array fixture has no expected mutation target.');
    }
    return Found;
}

function Run(Tool, Arguments) {
    const Result = spawnSync(Tool, Arguments, {
        encoding: 'utf8',
        maxBuffer: 65_536,
        timeout: 30_000,
        windowsHide: true,
    });
    if (Result.error !== undefined) {
        throw Result.error;
    }
    return Result;
}

function Requireˉaccepted(Path) {
    const Result = Run(Verifier, [Path]);
    if (Result.status !== 0 || Result.stderr !== '' ||
        Result.stdout.replaceAll('\r\n', '\n') !==
            'wvb status=Valid profile=compiler-aligned\n') {
        Reject(`${basename(Path)} was not accepted by the compiler-aligned verifier.`);
    }
}

function Requireˉrejected(Path) {
    const Result = Run(Verifier, [Path]);
    if (Result.status !== 1 || Result.stdout !== '' ||
        !/^wvb status=Invalid phase=[a-z-]+\r?\n$/u.test(Result.stderr)) {
        Reject(`${basename(Path)} was not rejected by the compiler-aligned verifier.`);
    }
}

try {
    const Module = Parseˉmodule(Validˉpath);
    const Code = Module.Sections[5];
    const Types = Module.Sections[7];
    const U64ˉone = Buffer.from([0x81, 1, 0, 0, 0, 0, 0, 0, 0]);
    const Indexˉconstant = Findˉunique(
        Module.Bytes,
        Code.payload,
        Code.length,
        U64ˉone,
    );
    const Arrayˉcreate = Findˉunique(
        Module.Bytes,
        Code.payload,
        Code.length,
        Buffer.from([0xc5, 0, 0, 0, 0]),
    );
    const Typesˉcursor = Types.payload;
    if (Module.Bytes.readUInt32LE(Typesˉcursor) !== 1 ||
        Module.Bytes[Typesˉcursor + 4] !== 4) {
        Reject('The fixed-array fixture does not declare one array type.');
    }
    const Nameˉbytes = Module.Bytes.readUInt32LE(Typesˉcursor + 5);
    const Elementˉshape = Typesˉcursor + 9 + Nameˉbytes;
    const Count = Elementˉshape + 1;
    if (Count + 4 !== Types.payload + Types.length ||
        Module.Bytes[Elementˉshape] !== 1 ||
        Module.Bytes.readUInt32LE(Count) !== 3) {
        Reject('The fixed-array fixture type descriptor is not Array<i32, 3>.');
    }

    Requireˉaccepted(Validˉpath);
    const Validˉrun = Run(Runner, [Validˉpath]);
    if (Validˉrun.status !== 0 || Validˉrun.stderr !== '' ||
        Validˉrun.stdout.replaceAll('\r\n', '\n') !== 'Result: 42\n') {
        Reject('The valid fixed-array fixture did not return exact result 42.');
    }

    const Bounds = Buffer.from(Module.Bytes);
    Bounds.writeBigUInt64LE(3n, Indexˉconstant + 1);
    writeFileSync(Mutations[0], Bounds);
    Requireˉaccepted(Mutations[0]);
    const Boundsˉrun = Run(Runner, [Mutations[0]]);
    if (Boundsˉrun.status !== 1 || Boundsˉrun.stdout !== '' ||
        !/^wvb run status=Failed code=3008 instructions=[0-9]+\r?\n$/u.test(
            Boundsˉrun.stderr,
        )) {
        Reject('The fixed-array bounds mutation did not trap with WVR3008.');
    }

    const Oldˉminor = Buffer.from(Module.Bytes);
    Oldˉminor.writeUInt16LE(16, 6);
    writeFileSync(Mutations[1], Oldˉminor);
    Requireˉrejected(Mutations[1]);

    const Largeˉcount = Buffer.from(Module.Bytes);
    Largeˉcount.writeUInt32LE(4096, Count);
    writeFileSync(Mutations[2], Largeˉcount);
    Requireˉrejected(Mutations[2]);

    const Unknownˉtype = Buffer.from(Module.Bytes);
    Unknownˉtype.writeUInt32LE(1, Arrayˉcreate + 1);
    writeFileSync(Mutations[3], Unknownˉtype);
    Requireˉrejected(Mutations[3]);

    process.stdout.write(
        'fixed array WVB verification status=Passed cases=6 result=42 ' +
        'bounds=WVR3008\n',
    );
} finally {
    for (const Mutation of Mutations) {
        try {
            unlinkSync(Mutation);
        } catch (Error) {
            if (Error?.code !== 'ENOENT') {
                throw Error;
            }
        }
    }
}
