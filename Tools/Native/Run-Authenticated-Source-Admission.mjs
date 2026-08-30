// Development verifier helper only. This retains authenticated phase artifacts
// for semantic inspection, but it is not an authoritative compiler ingress and
// never owns final WVB publication. Production compilation uses the bounded
// split-compiler coordinator and its independent WVSS republication check.
import { spawnSync } from 'node:child_process';
import {
    closeSync,
    lstatSync,
    openSync,
    readFileSync,
    realpathSync,
    unlinkSync,
    writeFileSync,
} from 'node:fs';
import path from 'node:path';

const MAXIMUM_VALUE_BYTES = 4_194_304;
const MAXIMUM_PRODUCT_BYTES = 134_217_728;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_PHASE_STREAM_BYTES = MAXIMUM_DIAGNOSTIC_BYTES / 2;
const TIMEOUT_MILLISECONDS = 300_000;

if (process.argv.length < 13 ||
    process.argv[4] !== '--source-input-lock' ||
    process.argv[7] !== '--source-profile' ||
    process.argv[9] !== '--target-descriptor') {
    Usage();
}

const Admitter = path.resolve(process.argv[2]);
const Validator = path.resolve(process.argv[3]);
const Lock = path.resolve(process.argv[5]);
const Lockˉdigest = process.argv[6];
const Profile = path.resolve(process.argv[8]);
const Target = path.resolve(process.argv[10]);
const Sourceˉpaths = process.argv.slice(11, -1).map(Candidate => path.resolve(Candidate));
const Admitted = path.resolve(process.argv.at(-1));
if (!/^[0-9a-f]{64}$/u.test(Lockˉdigest) || Sourceˉpaths.length > 64 ||
    path.extname(Admitted).toLowerCase() !== '.wvss') {
    Usage();
}

for (const [Candidate, Maximum, Label] of [
    [Admitter, MAXIMUM_PRODUCT_BYTES, 'admitter'],
    [Validator, MAXIMUM_PRODUCT_BYTES, 'validator'],
    [Target, 320, 'target descriptor'],
    [Lock, 1_048_576, 'source lock'],
    [Profile, 65_536, 'source profile'],
    ...Sourceˉpaths.map((Candidate, Index) =>
        [Candidate, MAXIMUM_VALUE_BYTES, `source module ${Index}`]),
]) {
    Requireˉordinaryˉfile(Candidate, Maximum, Label);
}

const Prefix = Admitted.slice(0, -'.wvss'.length);
const Input = `${Prefix}-Input.wvss`;
const Admittedˉtarget = `${Prefix}.wvtd`;
const Catalog = `${Prefix}.wvfc`;
const Evidence = `${Prefix}.wvae`;
const Retainedˉoutputs = [Admitted, Admittedˉtarget, Catalog, Evidence];
for (const Candidate of [Input, ...Retainedˉoutputs]) {
    if (Exists(Candidate)) Reject(`The admission output already exists: ${Candidate}.`);
}

let Authenticated = false;
let Ownsˉinput = false;
try {
    const Inputˉhandle = openSync(Input, 'wx', 0o600);
    Ownsˉinput = true;
    try {
        writeFileSync(Inputˉhandle, Constructˉsourceˉset(Sourceˉpaths));
    } finally {
        closeSync(Inputˉhandle);
    }
    const Admission = Run(Admitter, [
        '--source-input-lock', Lock, Lockˉdigest,
        '--source-profile', Profile,
        '--target-descriptor', Target,
        '--source-set', Input,
        Admitted, Admittedˉtarget, Catalog, Evidence,
    ]);
    Requireˉsuccess(Admission, 'source admission');
    const Authentication = Run(Validator, [
        Evidence, Admitted, Admittedˉtarget, Catalog, Lock, Profile,
    ]);
    Requireˉsuccess(Authentication, 'source authentication');
    Authenticated = true;
    process.stdout.write(Admission.stdout);
} finally {
    if (Ownsˉinput) {
        try { unlinkSync(Input); } catch (Error) {
            if (Error?.code !== 'ENOENT') throw Error;
        }
    }
    if (Ownsˉinput && !Authenticated) {
        for (const Candidate of Retainedˉoutputs) {
            try { unlinkSync(Candidate); } catch (Error) {
                if (Error?.code !== 'ENOENT') throw Error;
            }
        }
    }
}

function Constructˉsourceˉset(Paths) {
    const Headerˉbytes = 16 + Paths.length * 8;
    const Sources = [];
    let Payloadˉbytes = 0;
    for (const Candidate of Paths) {
        const Source = readFileSync(Candidate);
        if (Source.length > MAXIMUM_VALUE_BYTES - Headerˉbytes - Payloadˉbytes) {
            Reject('The source closure is outside the canonical WVSS bound.');
        }
        Payloadˉbytes += Source.length;
        Sources.push(Source);
    }
    const Result = Buffer.alloc(Headerˉbytes + Payloadˉbytes);
    Result.write('WVSS', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt32LE(Sources.length, 8);
    Result.writeUInt32LE(Sources.length * 8, 12);
    let Offset = Headerˉbytes;
    Sources.forEach((Source, Index) => {
        Result.writeUInt32LE(Offset, 16 + Index * 8);
        Result.writeUInt32LE(Source.length, 20 + Index * 8);
        Source.copy(Result, Offset);
        Offset += Source.length;
    });
    return Result;
}

function Run(Command, Arguments) {
    return spawnSync(Command, Arguments, {
        encoding: 'utf8',
        windowsHide: true,
        // spawnSync applies maxBuffer per stream. Halving it keeps the two
        // captured phase streams within one 65,536-byte helper envelope.
        maxBuffer: MAXIMUM_PHASE_STREAM_BYTES,
        timeout: TIMEOUT_MILLISECONDS,
    });
}

function Requireˉsuccess(Result, Label) {
    if (Result.error !== undefined || Result.status !== 0 || Result.stderr.length !== 0) {
        if (Result.stderr.length !== 0) process.stderr.write(Result.stderr);
        Reject(`The ${Label} product failed with status ${Result.status}.`);
    }
}

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        !Sameˉpath(realpathSync(Candidate), Candidate)) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}.`);
    }
}

function Exists(Candidate) {
    try { lstatSync(Candidate); return true; } catch (Error) {
        if (Error?.code === 'ENOENT') return false;
        throw Error;
    }
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === path.resolve(Right).toLowerCase()
        : Left === path.resolve(Right);
}

function Usage() {
    process.stderr.write(
        'Usage: node Run-Authenticated-Source-Admission.mjs ' +
        '<admitter> <validator> --source-input-lock ' +
        '<lock.wvlock> <sha256> --source-profile <profile.wvsp> ' +
        '--target-descriptor <target.wvtd> ' +
        '<root.wv> [dependency.wv ...] <admitted.wvss>\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
