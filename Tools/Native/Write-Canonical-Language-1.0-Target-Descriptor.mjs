import { closeSync, openSync, unlinkSync, writeFileSync } from 'node:fs';
import path from 'node:path';

const Explicit = process.argv.length === 5 && process.argv[2] === '--target';
const Outputˉargument = Explicit ? process.argv[4] : process.argv[2];
const Targetˉname = Explicit ? process.argv[3] : 'linux.x86_64.sysv_amd64_c_v1';
const Targets = new Map([
    ['windows.x86_64.none_v1', [1, 1, 1, 1, 64, 1, 0]],
    ['linux.x86_64.none_v1', [2, 2, 1, 1, 64, 1, 0]],
    ['linux.x86_64.sysv_amd64_c_v1', [4, 2, 1, 2, 64, 1, 1]],
]);
if ((!Explicit && process.argv.length !== 3) ||
    typeof Outputˉargument !== 'string' || path.extname(Outputˉargument).toLowerCase() !== '.wvtd' ||
    !Targets.has(Targetˉname)) {
    process.stderr.write(
        'Usage: node Write-Canonical-Language-1.0-Target-Descriptor.mjs ' +
        '[--target <windows.x86_64.none_v1|linux.x86_64.none_v1|' +
        'linux.x86_64.sysv_amd64_c_v1>] <new-target.wvtd>\n',
    );
    process.exit(64);
}

const Result = Buffer.alloc(64);
Result.write('WVTD', 0, 4, 'ascii');
Result.writeUInt16LE(1, 4);
Result.writeUInt32LE(64, 8);
const Values = Targets.get(Targetˉname);
Values.forEach((Value, Index) => {
    Result.writeUInt32LE(Value, 12 + Index * 4);
});
const Output = path.resolve(Outputˉargument);
let Ownsˉoutput = false;
let Published = false;
try {
    const Handle = openSync(Output, 'wx', 0o600);
    Ownsˉoutput = true;
    try {
        writeFileSync(Handle, Result);
    } finally {
        closeSync(Handle);
    }
    Published = true;
} finally {
    if (Ownsˉoutput && !Published) {
        try { unlinkSync(Output); } catch (Error) {
            if (Error?.code !== 'ENOENT') throw Error;
        }
    }
}
