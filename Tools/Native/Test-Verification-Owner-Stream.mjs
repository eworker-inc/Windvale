import {
    mkdirSync,
    mkdtempSync,
    rmSync,
    symlinkSync,
    writeFileSync,
} from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {
    Requireˉordinaryˉdirectoryˉpath,
    Requireˉordinaryˉnewˉpath,
} from './Verification-Owner-Stream-Path.mjs';

if (process.argv.length !== 2) {
    process.stderr.write(
        'Usage: node Tools/Native/Test-Verification-Owner-Stream.mjs\n'
    );
    process.exit(64);
}

const Work = mkdtempSync(path.join(
    os.tmpdir(), 'windvale-verification-owner-stream-',
));

try {
    const Fresh = path.join(Work, 'Fresh.log');
    if (await Requireˉordinaryˉnewˉpath(Fresh) !== path.resolve(Fresh)) {
        Reject('The fresh log path did not retain its resolved identity.');
    }
    Pass('ordinary-new-path');

    writeFileSync(Fresh, 'occupied', { encoding: 'utf8', flag: 'wx' });
    await Requireˉrejection(
        () => Requireˉordinaryˉnewˉpath(Fresh),
        'Owner log already exists:',
    );
    Pass('existing-path');

    const Ordinaryˉdirectory = path.join(Work, 'Ordinary');
    mkdirSync(Ordinaryˉdirectory);
    if (await Requireˉordinaryˉdirectoryˉpath(Ordinaryˉdirectory) !==
        path.resolve(Ordinaryˉdirectory)) {
        Reject('The ordinary directory did not retain its resolved identity.');
    }
    Pass('ordinary-directory');

    const Linkˉtarget = path.join(Work, 'Link-Target');
    const Link = path.join(Work, 'Link');
    mkdirSync(Linkˉtarget);
    symlinkSync(
        Linkˉtarget,
        Link,
        process.platform === 'win32' ? 'junction' : 'dir',
    );
    await Requireˉrejection(
        () => Requireˉordinaryˉnewˉpath(path.join(Link, 'Linked.log')),
        'Owner path must not traverse a link or non-directory:',
    );
    Pass('linked-parent-rejection');

    process.stdout.write(
        'verification owner stream status=Passed cases=4\n'
    );
} finally {
    rmSync(Work, { recursive: true, force: true });
}

async function Requireˉrejection(Action, Prefix) {
    try {
        await Action();
    } catch (Errorˉvalue) {
        if (Errorˉvalue instanceof Error &&
            Errorˉvalue.message.startsWith(Prefix)) {
            return;
        }
        throw Errorˉvalue;
    }
    Reject(`Expected rejection beginning with: ${Prefix}`);
}

function Pass(Name) {
    process.stdout.write(`PASS  verification owner stream case=${Name}\n`);
}

function Reject(Message) {
    throw new Error(Message);
}
