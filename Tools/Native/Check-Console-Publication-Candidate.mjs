import { open, stat } from 'node:fs/promises';
import { extname } from 'node:path';

const REJECTION = 'publication status=Rejected phase=console-application\n';
const WINDOWS_MAXIMUM_BYTES = 4_196_352;
const LINUX_MAXIMUM_BYTES = 4_202_608;

function Reject() {
    process.stderr.write(REJECTION);
    process.exit(1);
}

if (process.argv.length !== 3) {
    process.stderr.write(
        'Usage: node Tools/Native/Check-Console-Publication-Candidate.mjs ' +
        '<candidate.exe|candidate.elf>\n'
    );
    process.exit(64);
}

const Candidateˉpath = process.argv[2];
const Candidateˉkind = extname(Candidateˉpath).toLowerCase();
let Minimumˉbytes;
let Maximumˉbytes;
let Expectedˉmagic;
if (Candidateˉkind === '.exe') {
    Minimumˉbytes = 2_048;
    Maximumˉbytes = WINDOWS_MAXIMUM_BYTES;
    Expectedˉmagic = Uint8Array.of(0x4d, 0x5a);
} else if (Candidateˉkind === '.elf') {
    Minimumˉbytes = 5_120;
    Maximumˉbytes = LINUX_MAXIMUM_BYTES;
    Expectedˉmagic = Uint8Array.of(0x7f, 0x45, 0x4c, 0x46);
} else {
    Reject();
}

let Candidateˉstat;
try {
    Candidateˉstat = await stat(Candidateˉpath);
} catch {
    Reject();
}
if (!Candidateˉstat.isFile() ||
    Candidateˉstat.size < Minimumˉbytes ||
    Candidateˉstat.size > Maximumˉbytes) {
    Reject();
}

const Prefix = Buffer.alloc(Expectedˉmagic.length);
let Candidate;
let Bytesˉread = 0;
let Readˉfailed = false;
try {
    Candidate = await open(Candidateˉpath, 'r');
    ({ bytesRead: Bytesˉread } = await Candidate.read(
        Prefix,
        0,
        Prefix.length,
        0
    ));
} catch {
    Readˉfailed = true;
} finally {
    try {
        await Candidate?.close();
    } catch {
        Readˉfailed = true;
    }
}
if (Readˉfailed || Bytesˉread !== Prefix.length) {
    Reject();
}

for (let Index = 0; Index < Expectedˉmagic.length; Index += 1) {
    if (Prefix[Index] !== Expectedˉmagic[Index]) {
        Reject();
    }
}
