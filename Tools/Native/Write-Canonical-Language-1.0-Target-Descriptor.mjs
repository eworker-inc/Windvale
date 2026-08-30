import { closeSync, openSync, unlinkSync, writeFileSync } from 'node:fs';
import path from 'node:path';

if (process.argv.length !== 3 ||
    path.extname(process.argv[2]).toLowerCase() !== '.wvtd') {
    process.stderr.write(
        'Usage: node Write-Canonical-Language-1.0-Target-Descriptor.mjs ' +
        '<new-target.wvtd>\n',
    );
    process.exit(64);
}

const Result = Buffer.alloc(64);
Result.write('WVTD', 0, 4, 'ascii');
Result.writeUInt16LE(1, 4);
Result.writeUInt32LE(64, 8);
const Values = [4, 2, 1, 2, 64, 1, 1];
Values.forEach((Value, Index) => {
    Result.writeUInt32LE(Value, 12 + Index * 4);
});
const Output = path.resolve(process.argv[2]);
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
